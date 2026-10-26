const boardEl = document.querySelector("#board");
const catalogEl = document.querySelector("#catalog");
const statusEl = document.querySelector("#status");
const muteButton = document.querySelector("#mute");

const state = {
  match: null,
  catalog: [],
  seat: null,
  selectedTileId: null,
  selectedPerk: null,
  muted: false,
  previousOwners: new Map()
};

const audio = typeof window === "undefined" ? { play() {}, unlock() {} } : createAudio();

boot().catch((error) => setStatus(error.message, true));

muteButton?.addEventListener("click", () => {
  state.muted = !state.muted;
  muteButton.setAttribute("aria-pressed", String(state.muted));
  muteButton.textContent = state.muted ? "Ses kapalı" : "Ses açık";
});

document.querySelectorAll(".perk").forEach((button) => {
  button.addEventListener("click", () => selectPerk(button.dataset.perk));
});

document.querySelector("#abandon")?.addEventListener("click", () => {
  leaveTable().catch((error) => setStatus(error.message, true));
});

async function boot() {
  state.catalog = await api("/api/tiles");
  renderCatalog();
  document.querySelector("#host-form").addEventListener("submit", (event) => {
    event.preventDefault();
    hostRoom().catch((error) => setStatus(error.message, true));
  });
  document.querySelector("#join-form").addEventListener("submit", (event) => {
    event.preventDefault();
    joinRoom(new FormData(event.currentTarget).get("roomCode")).catch((error) => setStatus(error.message, true));
  });
  window.setInterval(paintClocks, 1000);
  const handed = sessionStorage.getItem("kiraathane-seat");
  if (handed) {
    state.seat = JSON.parse(handed);
    state.bot = new URLSearchParams(location.search).has("bot")
      ? JSON.parse(sessionStorage.getItem("kiraathane-bot") || "null")
      : null;
    if (!state.bot) sessionStorage.removeItem("kiraathane-bot");
    await loadMatch(state.seat.MatchId);
    const over = state.match?.Status === "Completed" || state.match?.Status === "Draw";
    if (over) {
      sessionStorage.removeItem("kiraathane-seat");
      if (state.bot) sessionStorage.setItem("kiraathane-rematch", JSON.stringify({
        level: state.bot.level,
        catalog: state.bot.catalog
      }));
      sessionStorage.removeItem("kiraathane-bot");
    }
    const ranks = { cirak: "Çırak", kalfa: "Kalfa", usta: "Usta" };
    setStatus(state.bot
      ? `${ranks[state.bot.level] || "Kalfa"} çaycı karşında. İlk yudum sende.`
      : `İkiniz de oturdunuz. ${state.match?.CurrentTurn === state.seat.Side ? "İlk yudum sende." : "İlk yudum karşıda."}`);
    return;
  }
  setStatus("Masa aç ya da oda koduyla otur.");
}

async function hostRoom() {
  state.seat = await api("/api/matches/rooms", { method: "POST" });
  sessionStorage.setItem("kiraathane-seat", JSON.stringify(state.seat));
  window.location.href = "/bekle.html?v=34";
}

async function restartMatch() {
  const curtain = document.querySelector("#curtain");
  const remembered = sessionStorage.getItem("kiraathane-rematch");
  const bot = state.bot || (remembered ? JSON.parse(remembered) : null);
  state.selectedTileId = null;
  state.selectedPerk = null;
  state.previousOwners = new Map();
  if (bot) {
    const light = await api("/api/matches/rooms?openingSide=Light&versusBot=true", { method: "POST" });
    const dark = await api("/api/matches/rooms/join", {
      method: "POST",
      body: JSON.stringify({ roomCode: light.RoomCode })
    });
    const catalog = bot.catalog?.length ? bot.catalog : await api("/api/tiles");
    state.seat = light;
    state.bot = {
      matchId: light.MatchId,
      seatToken: dark.SeatToken,
      side: dark.Side,
      level: bot.level || "kalfa",
      catalog
    };
    sessionStorage.setItem("kiraathane-seat", JSON.stringify(state.seat));
    sessionStorage.setItem("kiraathane-bot", JSON.stringify(state.bot));
    sessionStorage.setItem("kiraathane-human", JSON.stringify(light));
    sessionStorage.removeItem("kiraathane-rematch");
    const url = new URL(location.href);
    url.searchParams.set("bot", "1");
    history.replaceState(null, "", url);
    await loadMatch(light.MatchId);
    curtain.hidden = true;
    const ranks = { cirak: "Çırak", kalfa: "Kalfa", usta: "Usta" };
    setStatus(`${ranks[state.bot.level] || "Kalfa"} çaycı yeni fincanları dizdi. İlk yudum sende.`);
    return;
  }
  curtain.hidden = true;
  await hostRoom();
}

async function leaveTable() {
  const button = document.querySelector("#abandon");
  if (!state.seat || !state.match || !state.connection) return;
  const over = state.match.Status === "Completed" || state.match.Status === "Draw";
  if (over) return;
  const rival = state.bot ? "Çaycı" : "Rakip";
  if (!window.confirm(`${rival} masayı alır. Terk edilsin mi?`)) return;
  button.disabled = true;
  const token = state.bot ? state.bot.seatToken : state.seat.SeatToken;
  if (state.bot) sessionStorage.setItem("kiraathane-rematch", JSON.stringify({
    level: state.bot.level,
    catalog: state.bot.catalog
  }));
  await state.connection.invoke("Leave", state.seat.MatchId, token);
  sessionStorage.removeItem("kiraathane-seat");
  sessionStorage.removeItem("kiraathane-bot");
  state.bot = null;
}

async function joinRoom(code) {
  state.seat = await api("/api/matches/rooms/join", {
    method: "POST",
    body: JSON.stringify({ roomCode: String(code || "").trim().toUpperCase() })
  });
  await loadMatch(state.seat.MatchId);
  const mine = state.seat.Side === "Light" ? "Açık" : "Koyu";
  const opens = state.match?.CurrentTurn === state.seat.Side;
  setStatus(`${mine} koltuğa oturdun. ${opens ? "İlk yudum sende." : "İlk yudum karşıda."}`);
}

async function loadMatch(matchId) {
  state.match = await api(`/api/matches/${matchId}`);
  rememberOwners();
  render();
  await connect(matchId);
}

async function connect(matchId) {
  if (!window.signalR) return;
  if (state.connection) await state.connection.stop();
  const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/matches").withAutomaticReconnect().build();
  connection.on("state", (next) => applyMatch(normalize(next), false));
  connection.onreconnected(async () => {
    if (!state.seat || state.bot) return;
    await connection.invoke("Sit", state.seat.MatchId, state.seat.SeatToken);
  });
  connection.onclose(() => {
    state.connection = null;
  });
  window.addEventListener("pagehide", () => {
    if (!state.connection || !state.seat || state.bot) return;
    const over = state.match && (state.match.Status === "Completed" || state.match.Status === "Draw");
    if (over) return;
    state.connection.invoke("Leave", state.seat.MatchId, state.seat.SeatToken).catch(() => {});
  });
  await connection.start();
  if (state.seat && !state.bot)
    await connection.invoke("Sit", matchId, state.seat.SeatToken);
  else
    await connection.invoke("JoinMatch", matchId);
  state.connection = connection;
}

const copiesPerTile = 2;

function copiesLeft(tileId, side) {
  const row = state.match?.Stock?.find((item) => item.TileDefinitionId === tileId);
  if (!row) return copiesPerTile;
  return side === "Dark" ? row.DarkLeft : row.LightLeft;
}

function renderCatalog() {
  const side = state.seat?.Side || "Light";
  const ordered = [...state.catalog]
    .filter((tile) => copiesLeft(tile.Id, side) > 0)
    .sort((a, b) => a.Value - b.Value || a.Name.localeCompare(b.Name, "tr"));
  if (state.selectedTileId && !ordered.some((tile) => tile.Id === state.selectedTileId))
    state.selectedTileId = null;
  catalogEl.replaceChildren(...ordered.map((tile) => {
    const left = copiesLeft(tile.Id, side);
    const button = document.createElement("button");
    button.type = "button";
    button.className = "tile";
    if (tile.Id === state.selectedTileId) button.classList.add("selected");
    button.dataset.id = tile.Id;
    button.setAttribute("aria-label", `${tile.Name}, ${left} kaldı, kuzey ${tile.North}, doğu ${tile.East}, güney ${tile.South}, batı ${tile.West}`);
    button.innerHTML = `<span class="tile-face"><strong>${tile.Name}<em>×${left}</em></strong><span class="cup" aria-hidden="true"><i class="n">${tile.North}</i><i class="e">${tile.East}</i><i class="s">${tile.South}</i><i class="w">${tile.West}</i><b>${tile.Value}</b></span></span>`;
    button.addEventListener("click", () => selectTile(tile.Id));
    button.querySelector(".cup").addEventListener("pointerdown", (event) => beginDrag(event, tile));
    return button;
  }));
}

function selectTile(id) {
  state.selectedTileId = id;
  document.querySelectorAll(".tile").forEach((item) => item.classList.toggle("selected", item.dataset.id === id));
}

const drag = { pointerId: null, tile: null, ghost: null, origin: null, moved: false };

function beginDrag(event, tile) {
  if (event.button !== undefined && event.button !== 0) return;
  if (!state.seat || state.match?.Status !== "InProgress" || state.match.CurrentTurn !== state.seat.Side) return;
  if (copiesLeft(tile.Id, state.seat.Side) === 0) return;
  selectTile(tile.Id);
  drag.pointerId = event.pointerId;
  drag.tile = tile;
  drag.moved = false;
  drag.origin = event.currentTarget.closest(".tile");
  drag.startX = event.clientX;
  drag.startY = event.clientY;
  document.addEventListener("pointermove", moveDrag);
  document.addEventListener("pointerup", endDrag);
  document.addEventListener("pointercancel", endDrag);
}

function moveDrag(event) {
  if (event.pointerId !== drag.pointerId) return;
  const travel = Math.hypot(event.clientX - drag.startX, event.clientY - drag.startY);
  if (!drag.moved && travel < 8) return;
  if (!drag.moved) {
    drag.moved = true;
    event.preventDefault();
    drag.origin.classList.add("dragging");
    const source = drag.origin.querySelector(".cup");
    const box = source.getBoundingClientRect();
    drag.ghost = source.cloneNode(true);
    drag.ghost.classList.add("drag-ghost");
    document.body.append(drag.ghost);
  }
  moveGhost(event.clientX, event.clientY);
  const cell = cellUnder(event.clientX, event.clientY);
  document.querySelectorAll(".cell.drop-target").forEach((item) => item.classList.remove("drop-target"));
  if (cell && !cell.classList.contains("filled")) cell.classList.add("drop-target");
}

function moveGhost(x, y) {
  drag.ghost.style.transform = `translate(${x}px, ${y}px) translate(-50%, -50%)`;
}

function cellUnder(x, y) {
  const ghost = drag.ghost;
  const hidden = ghost?.style.visibility;
  if (ghost) ghost.style.visibility = "hidden";
  const cell = document.elementFromPoint(x, y)?.closest(".cell");
  if (ghost) ghost.style.visibility = hidden || "";
  return cell;
}

function endDrag(event) {
  if (event.pointerId !== drag.pointerId) return;
  document.removeEventListener("pointermove", moveDrag);
  document.removeEventListener("pointerup", endDrag);
  document.removeEventListener("pointercancel", endDrag);
  const cell = cellUnder(event.clientX, event.clientY);
  document.querySelectorAll(".cell.drop-target").forEach((item) => item.classList.remove("drop-target"));
  drag.ghost?.remove();
  drag.origin.classList.remove("dragging");
  const landed = drag.moved && cell && !cell.classList.contains("filled");
  drag.pointerId = null;
  drag.ghost = null;
  drag.origin = null;
  if (landed) place(Number(cell.style.gridRow) - 1, Number(cell.style.gridColumn) - 1);
}

function render() {
  const match = state.match;
  document.body.classList.toggle("in-match", Boolean(match));
  const abandon = document.querySelector("#abandon");
  if (abandon) {
    const over = match.Status === "Completed" || match.Status === "Draw";
    abandon.hidden = !state.seat || over;
    abandon.disabled = false;
  }
  paintSide("light", match.LightScore, match.LightPerkCharges, match.CurrentTurn === "Light");
  paintSide("dark", match.DarkScore, match.DarkPerkCharges, match.CurrentTurn === "Dark");
  document.querySelector("#turn-pill").textContent = turnLabel(match);

  const cells = [...match.Cells].sort((a, b) => a.Row - b.Row || a.Column - b.Column);
  boardEl.replaceChildren(...cells.map((cell) => renderCell(cell)));
  renderCatalog();
  const mine = state.seat?.Side === "Dark" ? match.DarkPerkCharges : match.LightPerkCharges;
  document.querySelectorAll(".perk").forEach((button) => {
    button.classList.toggle("selected", button.dataset.perk === state.selectedPerk);
    button.disabled = mine === 0 || match.Status !== "InProgress";
  });
}

function renderCell(cell) {
  const button = document.createElement("button");
  button.type = "button";
  button.className = "cell";
  button.setAttribute("role", "gridcell");
  button.style.gridRow = cell.Row + 1;
    button.style.gridColumn = cell.Column + 1;
    const key = `${cell.Row}:${cell.Column}`;
    const empty = cell.Owner == null;

    if (!empty) {
      button.classList.add("filled", cell.Owner === "Light" ? "owner-light" : "owner-dark");
      if (cell.IsFaceDown) button.classList.add("face-down");
      if (cell.IsLocked) button.classList.add("locked");
      if (state.previousOwners.get(key) === "empty") button.classList.add("just-placed");
      else if (state.previousOwners.has(key) && state.previousOwners.get(key) !== cell.Owner) button.classList.add("just-flipped");
      button.disabled = true;
      const ownerName = cell.Owner === "Light" ? "Açık" : "Koyu";
      const stateName = cell.IsFaceDown ? ", kapalı" : cell.IsLocked ? ", kilitli" : "";
      button.setAttribute("aria-label", `${ownerName} taşı, satır ${cell.Row + 1}, sütun ${cell.Column + 1}${stateName}`);
      const face = cell.IsFaceDown
        ? `<span class="value" aria-hidden="true">?</span>`
        : `<span class="edge n" aria-hidden="true">${cell.North || ""}</span><span class="edge e" aria-hidden="true">${cell.East || ""}</span><span class="edge s" aria-hidden="true">${cell.South || ""}</span><span class="edge w" aria-hidden="true">${cell.West || ""}</span><span class="value" aria-hidden="true">${cell.Value}</span>${cell.IsLocked ? `<span class="lock" aria-hidden="true"></span>` : ""}`;
      button.innerHTML = `<span class="piece">${face}</span>`;
      return button;
    }

    button.setAttribute("aria-label", `Satır ${cell.Row + 1}, sütun ${cell.Column + 1}`);
    button.addEventListener("click", () => place(cell.Row, cell.Column));
  return button;
}

async function place(row, column) {
  if (!state.seat || state.match?.Status === "Waiting") {
    setStatus("Rakip oturmadan taş konmaz.", true);
    return;
  }
  if (state.match.CurrentTurn !== state.seat.Side) {
    setStatus("Sıra sende değil.", true);
    return;
  }
  if (!state.selectedTileId) {
    setStatus("Önce elinden bir taş seç.", true);
    return;
  }
  if (copiesLeft(state.selectedTileId, state.seat.Side) === 0) {
    setStatus("Bu taştan ikisini de kullandın.", true);
    return;
  }

  const perk = state.selectedPerk === "LafEbeligi" ? null : state.selectedPerk;
  try {
    const next = await api(`/api/matches/${state.match.Id}/tiles`, {
      method: "POST",
      body: JSON.stringify({
            matchId: state.match.Id,
        tileDefinitionId: state.selectedTileId,
            side: state.seat.Side,
            seatToken: state.seat.SeatToken,
        row,
        column,
        perk
      })
    });
    applyMatch(next, true);
    setStatus(perk === "GizliHamle" ? "Kapalı kondu." : "Taş masaya indi.");
  } catch (error) {
    setStatus(error.message, true);
  }
}

async function selectPerk(perk) {
  if (perk === "LafEbeligi") {
    try {
      const next = await api(`/api/matches/${state.match.Id}/perks`, {
        method: "POST",
              body: JSON.stringify({ matchId: state.match.Id, side: state.seat.Side, seatToken: state.seat.SeatToken, perk })
      });
      applyMatch(next, false);
      setStatus("Laf Ebeliği saati 15 saniye kısalttı.");
    } catch (error) {
      setStatus(error.message, true);
    }
    return;
  }

  state.selectedPerk = state.selectedPerk === perk ? null : perk;
  render();
}

function applyMatch(next, fromPlacement) {
  const flips = (next.Events ?? []).filter((event) => event.Kind === "TilesFlipped" || event.Kind === "TileRevealed").length;
  next.receivedAt = Date.now();
  state.match = next;
  render();
  rememberOwners();
  state.selectedPerk = null;
  showCurtain(next);
  if (state.bot && next.Status === "InProgress" && next.CurrentTurn === state.bot.side) {
    window.setTimeout(() => botMove().catch((error) => setStatus(error.message, true)), 700);
  }
  if (state.muted || !fromPlacement) return;
  audio.thud();
  if (flips > 0) window.setTimeout(() => audio.chime(), reducedMotion() ? 0 : 180);
}

async function botMove() {
  const match = state.match;
  const bot = state.bot;
  if (!bot || match.Status !== "InProgress" || match.CurrentTurn !== bot.side || state.botThinking) return;
  state.botThinking = true;
  try {
    const choice = chooseBotMove(match, bot);
    const next = await api(`/api/matches/${match.Id}/tiles`, {
      method: "POST",
      body: JSON.stringify({
        matchId: match.Id,
        tileDefinitionId: choice.tile.Id,
        side: bot.side,
        seatToken: bot.seatToken,
        row: choice.spot.Row,
        column: choice.spot.Column
      })
    });
    applyMatch(next, true);
    setStatus("Çaycı bir fincan koydu.");
  } finally {
    state.botThinking = false;
  }
}

function chooseBotMove(match, bot) {
  const level = bot.level || "kalfa";
  const empty = match.Cells.filter((cell) => cell.Owner == null);
  const opponent = bot.side === "Light" ? "Dark" : "Light";
  const moves = [];
  const hand = bot.catalog.filter((tile) => copiesLeft(tile.Id, bot.side) > 0);
  for (const tile of hand) {
    for (const spot of empty) {
      const captured = capturesAt(match, tile, spot, bot.side);
      const exposed = threatAt(match, bot, tile, spot, opponent);
      moves.push({ tile, spot, captured, exposed, score: captured * 10 - exposed });
    }
  }
  if (level === "cirak") {
    const blind = moves.filter((move) => move.captured === 0);
    const pool = blind.length ? blind : moves;
    return pool[Math.floor(Math.random() * pool.length)];
  }
  const bestGain = Math.max(...moves.map((move) => move.captured));
  if (level === "kalfa") {
    const greedy = bestGain > 0 ? moves.filter((move) => move.captured === bestGain) : moves;
    return greedy[Math.floor(Math.random() * greedy.length)];
  }
  const safest = Math.max(...moves.map((move) => move.score));
  const careful = moves.filter((move) => move.score === safest);
  return careful[Math.floor(Math.random() * careful.length)];
}

function capturesAt(match, tile, spot, side) {
  return neighborsOf(spot).filter(({ cell, attack, defense }) => {
    if (!cell || cell.Owner == null || cell.Owner === side || cell.IsLocked || cell.IsFaceDown) return false;
    const edge = tile[attack];
    return edge > cell[defense] || (edge === cell[defense] && edge > 0 && tile.Value > cell.Value);
  }).length;
}

function threatAt(match, bot, tile, spot, opponent) {
  if (!match.Cells.some((cell) => cell.Owner === opponent)) return 0;
  return neighborsOf(spot).filter(({ cell, attack, defense }) => {
    if (cell?.Owner != null) return false;
    const replies = bot.catalog.filter((tile) => copiesLeft(tile.Id, opponent) > 0);
    return replies.some((reply) => {
      const edge = reply[defense];
      return edge > tile[attack] || (edge === tile[attack] && edge > 0 && reply.Value > tile.Value);
    });
  }).length;
}

function neighborsOf(spot) {
  return [
    ["North", "South", spot.Row - 1, spot.Column],
    ["East", "West", spot.Row, spot.Column + 1],
    ["South", "North", spot.Row + 1, spot.Column],
    ["West", "East", spot.Row, spot.Column - 1]
  ].map(([attack, defense, row, column]) => ({
    attack,
    defense,
    cell: row < 0 || column < 0 || row > 4 || column > 4
      ? null
      : state.match.Cells.find((cell) => cell.Row === row && cell.Column === column)
  }));
}

function showCurtain(match) {
  const curtain = document.querySelector("#curtain");
  if (!curtain) return;
  const over = match.Status === "Completed" || match.Status === "Draw";
  curtain.hidden = !over;
  if (!over) return;
  const mine = state.seat?.Side;
  const won = match.Winner === mine;
  const lost = match.Status === "Completed" && match.Winner && match.Winner !== mine;
  const rival = state.bot ? "Çaycı" : mine === "Light" ? "Koyu" : "Açık";
  const slippedOut = match.Walkover === true;
  const lines = slippedOut && won
    ? ["Çayını içmeden kalktı", `${rival} kapıya yöneldi, hesabı da sana bıraktı. Masa senin.`, "Rakip kaçtı"]
    : slippedOut && lost
      ? ["Kapıyı sen çektin", "Fincan masada kaldı. Çaycı bu turu karşıya yazdı.", "Erken kalktın"]
      : won
      ? ["Hesap sende değil", "Fincanlar senin tarafına döndü. Çaycı bu turu deftere yazacak.", "Masa senin"]
      : lost
        ? ["Çay soğudu", `${rival} fincanları topladı. Bir yudum daha, bu sefer kenara bak.`, "Hesap geldi"]
        : ["İki çay, bir hesap", "Ne sen devirdin ne o. Bardaklar ortada kaldı.", "Berabere"];
  if (won && window.KiraathaneAccount?.noteWin) window.KiraathaneAccount.noteWin(match.Id);
  curtain.className = `curtain ${won ? "win" : lost ? "lose" : "draw"}`;
  document.querySelector("#curtain-kicker").textContent = lines[2];
  document.querySelector("#curtain-title").textContent = lines[0];
  document.querySelector("#curtain-line").textContent = lines[1];
  document.querySelector("#curtain-score").textContent = `${match.LightScore} – ${match.DarkScore}`;
  bindWallSign(match);
  const again = document.querySelector("#curtain-again");
  again.onclick = () => {
    again.disabled = true;
    restartMatch()
      .catch((error) => setStatus(error.message, true))
      .finally(() => { again.disabled = false; });
  };
  window.setTimeout(() => again.focus(), reducedMotion() ? 0 : 540);
}

function bindWallSign(match) {
  const form = document.querySelector("#curtain-sign");
  const signed = document.querySelector("#curtain-signed");
  if (!form) return;
  const human = !state.bot && (match.Status === "Completed" || match.Status === "Draw");
  form.hidden = !human;
  signed.hidden = true;
  if (!human) return;
  const account = readAccount();
  if (!signed) return;
  if (!account) {
    signed.hidden = false;
    signed.innerHTML = `Duvara yazılmak için <a href="/kapi.html">kapıdan gir</a>. Çaycı masası yine sayılmaz.`;
    return;
  }
  signed.hidden = false;
  signed.textContent = "Duvara yazılıyor…";
  api(`/api/matches/${match.Id}/wall`, {
    method: "POST",
    body: JSON.stringify({
      matchId: match.Id,
      seatToken: state.seat.SeatToken,
      name: account.FullName || account.RegisteredName
    })
  }).then(() => {
    signed.textContent = `${account.FullName || account.RegisteredName} duvara yazıldı.`;
  }).catch((error) => {
    signed.textContent = error.message;
  });
}

function readAccount() {
  try {
    const account = JSON.parse(localStorage.getItem("kiraathane-account") || "null");
    if (!account?.AccessToken || Date.parse(account.ExpiresAt) <= Date.now()) return null;
    return account;
  } catch {
    return null;
  }
}

function rememberOwners() {
  state.previousOwners = new Map(state.match.Cells.map((cell) => [
      `${cell.Row}:${cell.Column}`,
      cell.Owner == null ? "empty" : cell.Owner
  ]));
}

function paintSide(side, score, charges, active) {
  document.querySelector(`#score-${side}`).textContent = score;
  paintClocks();
  document.querySelector(`#side-${side}`).classList.toggle("active", active);
  const holder = document.querySelector(`#charges-${side}`);
  const name = side === "light" ? "Açık" : "Koyu";
  holder.setAttribute("aria-label", `${name} joker hakkı, ${charges} kaldı`);
  holder.replaceChildren(...Array.from({ length: 3 }, (_, index) => {
    const mark = document.createElement("i");
    if (index >= charges) mark.className = "spent";
    return mark;
  }), Object.assign(document.createElement("b"), { textContent: String(charges) }));
}

function paintClocks() {
  const match = state.match;
  if (!match) return;
  const drift = match.receivedAt && match.Status === "InProgress"
    ? Math.max(0, Math.floor((Date.now() - match.receivedAt) / 1000))
    : 0;
  const light = match.CurrentTurn === "Light"
    ? Math.max(0, match.LightSecondsRemaining - drift)
    : match.LightSecondsRemaining;
  const dark = match.CurrentTurn === "Dark"
    ? Math.max(0, match.DarkSecondsRemaining - drift)
    : match.DarkSecondsRemaining;
  document.querySelector("#clock-light").textContent = formatClock(light);
  document.querySelector("#clock-dark").textContent = formatClock(dark);
}

function formatClock(total) {
  const minutes = Math.floor(total / 60);
  const seconds = total % 60;
  return `${minutes}:${String(seconds).padStart(2, "0")}`;
}

function turnLabel(match) {
  if (match.Status === "Waiting") return "Rakip bekleniyor";
  if (match.Walkover && match.Status === "Completed") {
    return match.Winner === "Light" ? "Koyu masayı bıraktı" : "Açık masayı bıraktı";
  }
  if (match.Status === "Completed" && match.Winner === "Light") return "Açık kazandı";
    if (match.Status === "Completed" && match.Winner === "Dark") return "Koyu kazandı";
    if (match.Status === "Draw") return "Berabere";
    return match.CurrentTurn === "Light" ? "Sıra açıkta" : "Sıra koyuda";
}

async function api(url, options = {}) {
  const account = readAccount();
  const response = await fetch(url, {
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      ...(account ? { Authorization: `Bearer ${account.AccessToken}` } : {})
    },
    ...options
  });
  const body = await response.json();
  if (!response.ok || body.success === false) {
    const errors = body.errors ?? body.Errors;
    throw new Error(errors?.[0]?.message || errors?.[0]?.Message || body.message || body.Message || "Masa bu hamleyi kabul etmedi.");
  }
  return normalize(body.data ?? body.Data);
}

// HTTP is camelCase; SignalR may still arrive as PascalCase. Uppercasing an
// already-capital name doubled it (LightSecondsRemaining) and blanked the clocks.
function normalize(value) {
  if (Array.isArray(value)) return value.map(normalize);
  if (value == null || typeof value !== "object") return value;
  return Object.fromEntries(Object.entries(value).map(([key, item]) => {
    const name = key.length && key[0] === key[0].toLowerCase()
      ? key[0].toUpperCase() + key.slice(1)
      : key;
    return [name, normalize(item)];
  }));
}

function setStatus(message, isError = false) {
  statusEl.textContent = message;
  statusEl.classList.toggle("error", isError);
}

function reducedMotion() {
  return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

function createAudio() {
  const context = new (window.AudioContext || window.webkitAudioContext)();
  const tone = (frequency, duration, type, gainValue) => {
    if (context.state === "suspended") context.resume();
    const now = context.currentTime;
    const oscillator = context.createOscillator();
    const gain = context.createGain();
    oscillator.type = type;
    oscillator.frequency.setValueAtTime(frequency, now);
    gain.gain.setValueAtTime(gainValue, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + duration);
    oscillator.connect(gain).connect(context.destination);
    oscillator.start(now);
    oscillator.stop(now + duration);
  };

  return {
    thud: () => tone(110, 0.12, "triangle", 0.08),
    chime: () => tone(880, 0.18, "sine", 0.05)
  };
}
