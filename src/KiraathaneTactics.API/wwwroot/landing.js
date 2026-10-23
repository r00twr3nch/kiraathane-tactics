const note = document.querySelector("#queue-note");
const dialog = document.querySelector("#queue");
const queueCopy = document.querySelector("#queue-copy");
const queueCode = document.querySelector("#queue-code");
const findButton = document.querySelector("#find-rival");
const botButton = document.querySelector("#play-bot");
const hostButton = document.querySelector("#host-room");
const joinForm = document.querySelector("#join-room");
const tableForm = document.querySelector("#table-form");
const clockRange = document.querySelector("#match-length");
const clockReadout = document.querySelector("#clock-readout");
const board = document.querySelector("#leaderboard");

let polling = null;
let offered = null;

if (board) renderBoard();
if (tableForm) bindTableForm();
paintAccount();
document.addEventListener("DOMContentLoaded", paintAccount);

findButton?.addEventListener("click", () => findRival().catch(fail));
botButton?.addEventListener("click", () => playBot().catch(fail));
joinForm?.addEventListener("submit", (event) => {
  event.preventDefault();
  joinRoom(new FormData(joinForm).get("roomCode")).catch(fail);
});
document.querySelector("#queue-cancel")?.addEventListener("click", cancelQueue);

function renderBoard() {
  board.innerHTML = `<li class="empty"><b>Duvar bakılıyor</b><span>İsimler masadan geliyor.</span></li>`;
  api("/api/matches/wall")
    .then((wall) => paintBoard(wall.Rows || wall.rows || []))
    .catch(() => {
      board.innerHTML = `<li class="empty"><b>Duvar kapalı</b><span>Masa şu an cevap vermiyor.</span></li>`;
    });
}

function paintBoard(rows) {
  const podium = document.querySelector("#podium");
  if (!rows.length) {
    if (podium) {
      podium.hidden = true;
      podium.replaceChildren();
    }
    board.classList.remove("quiet");
    board.innerHTML = `<li class="empty"><b>Duvar boş</b><span>İlk insan maçını bitiren adını buraya yazar. Çaycı sayılmaz.</span></li>`;
    return;
  }
  const shown = rows.slice(0, 8);
  if (podium) {
    podium.hidden = false;
    const seats = [shown[1], shown[0], shown[2]].filter(Boolean);
    podium.replaceChildren(...seats.map((row) => {
      const place = shown.indexOf(row) + 1;
      const item = document.createElement("li");
      item.className = `seat seat-${place}`;
      item.innerHTML = `<span class="medal" aria-hidden="true">${place}</span><strong>${escapeHtml(row.Name || row.name)}</strong><em>${row.Points ?? row.points}</em><small>${wallDetail(row)}</small>`;
      return item;
    }));
  }
  const rest = shown.slice(3);
  board.classList.toggle("quiet", rest.length === 0);
  board.replaceChildren(...rest.map((row, index) => {
    const item = document.createElement("li");
    item.innerHTML = `<span class="rank">${index + 4}</span><span class="who"><strong>${escapeHtml(row.Name || row.name)}</strong><small>${wallDetail(row)}</small></span><span class="score">${row.Points ?? row.points}</span>`;
    return item;
  }));
}

function wallDetail(row) {
  const wins = row.Wins ?? row.wins ?? 0;
  const draws = row.Draws ?? row.draws ?? 0;
  const losses = row.Losses ?? row.losses ?? 0;
  return `${wins} galibiyet · ${draws} beraberlik · ${losses} mağlubiyet`;
}

async function findRival() {
  setBusy(true);
  note.textContent = "Rakip aranıyor";
  const response = await fetch("/api/matches/queue", {
    method: "POST",
    headers: { Accept: "application/json" }
  });
  const body = await response.json();
  if (!response.ok || body.success === false) {
    const errors = body.errors ?? body.Errors;
    throw new Error(errors?.[0]?.message || errors?.[0]?.Message || body.message || body.Message || "Masa cevap vermedi.");
  }

  const seat = normalize(body.data ?? body.Data);
  if (response.status === 202) {
    offered = seat;
    note.textContent = "Rakip aranıyor";
    queueCopy.textContent = "Rakip aranıyor";
    queueCode.hidden = true;
    if (typeof dialog.showModal === "function") dialog.showModal();
    dialog.classList.add("searching");
    polling = window.setInterval(() => watchTable(seat).catch(fail), 1500);
    return;
  }

  note.textContent = "Rakip bulundu, oturuluyor.";
  dialog?.classList.remove("searching");
  if (dialog?.open) dialog.close();
  goPlay(seat);
}

async function watchTable(seat) {
  const match = await api(`/api/matches/${seat.MatchId}`);
  if (match.Status === "Waiting") return;
  window.clearInterval(polling);
  offered = null;
  dialog?.classList.remove("searching");
  if (dialog?.open) dialog.close();
  goPlay(seat);
}

function bindTableForm() {
  const paint = () => {
    const minutes = Number(clockRange.value);
    const seconds = String((minutes % 1) * 60).padStart(2, "0");
    clockReadout.textContent = `${Math.floor(minutes)}:${seconds}`;
  };
  clockRange.addEventListener("input", paint);
  paint();
  tableForm.addEventListener("submit", (event) => {
    event.preventDefault();
    const data = new FormData(tableForm);
    hostRoom({
      color: data.get("color"),
      opening: data.get("opening"),
      minutes: Number(clockRange.value)
    }).catch(fail);
  });
}

async function hostRoom(settings) {
  setBusy(true);
  note.textContent = "Oda kuruluyor.";
  const hostDark = settings.color === "Dark";
  let opening = "";
  if (settings.opening === "host") opening = `&openingSide=${hostDark ? "Dark" : "Light"}`;
  if (settings.opening === "guest") opening = `&openingSide=${hostDark ? "Light" : "Dark"}`;
  const seat = await api(`/api/matches/rooms?matchMinutes=${settings.minutes}&hostColor=${settings.color}${opening}`, { method: "POST" });
  sessionStorage.setItem("kiraathane-seat", JSON.stringify(seat));
  window.location.href = `/bekle.html?v=34`;
}

async function joinRoom(code) {
  const roomCode = String(code || "").trim().toUpperCase();
  if (!roomCode) throw new Error("Oda kodunu yaz.");
  setBusy(true);
  note.textContent = "Odaya giriliyor.";
  const seat = await api("/api/matches/rooms/join", {
    method: "POST",
    body: JSON.stringify({ roomCode })
  });
  goPlay(seat);
}

async function playBot() {
  setBusy(true);
  note.textContent = "Çaycı önlüğü bağlıyor.";
  const light = await api("/api/matches/rooms?openingSide=Light&versusBot=true", { method: "POST" });
  const dark = await api("/api/matches/rooms/join", {
    method: "POST",
    body: JSON.stringify({ roomCode: light.RoomCode })
  });
  const catalog = await api("/api/tiles");
  const level = document.querySelector("input[name='level']:checked")?.value || "kalfa";
  sessionStorage.setItem("kiraathane-bot", JSON.stringify({
    matchId: light.MatchId,
    seatToken: dark.SeatToken,
    side: dark.Side,
    level,
    catalog
  }));
  sessionStorage.setItem("kiraathane-human", JSON.stringify(light));
  goPlay(light, true);
}

function goPlay(seat, versusBot) {
  sessionStorage.setItem("kiraathane-seat", JSON.stringify(seat));
  const bot = versusBot ? "&bot=1" : "";
  window.location.href = `/play/index.html?v=34&from=landing${bot}`;
}

const waitCode = document.querySelector("#wait-code");
if (waitCode) holdTable();

function holdTable() {
  const raw = sessionStorage.getItem("kiraathane-seat");
  const seat = raw ? JSON.parse(raw) : null;
  const note = document.querySelector("#queue-note");
  const facts = document.querySelector("#wait-facts");
  const pulse = document.querySelector("#wait-pulse");
  const veil = document.querySelector("#wait-veil");
  if (!seat?.MatchId || !seat.RoomCode) {
    note.textContent = "Açık masa yok. Önce masayı kur.";
    window.setTimeout(() => { window.location.href = "/masa.html"; }, 900);
    return;
  }

  waitCode.textContent = seat.RoomCode;
  const sideName = seat.Side === "Dark" ? "Koyu" : "Açık";
  facts.innerHTML = `<li><span>Senin rengin</span><strong>${sideName}</strong></li><li><span>Durum</span><strong>Kapı açık</strong></li>`;
  document.querySelector("#wait-copy").addEventListener("click", async () => {
    try {
      await navigator.clipboard.writeText(seat.RoomCode);
      note.textContent = "Kod kopyalandı.";
    } catch {
      note.textContent = "Kodu elle seç: " + seat.RoomCode;
    }
  });
  document.querySelector("#wait-leave").addEventListener("click", () => {
    sessionStorage.removeItem("kiraathane-seat");
  });
  const sip = document.querySelector("#tea-sip");
  const toast = document.querySelector("#tea-toast");
  let sipping = false;
  sip?.addEventListener("click", () => {
    if (sipping) return;
    sipping = true;
    sip.classList.add("stirring");
    stirTea();
    const spot = sip.getBoundingClientRect();
    toast.style.top = `${spot.top - 8}px`;
    toast.style.transform = "translate(-50%, -100%)";
    document.body.appendChild(toast);
    toast.hidden = false;
    window.setTimeout(() => {
      sip.classList.remove("stirring");
      toast.hidden = true;
      sipping = false;
    }, reducedWait() ? 400 : 2200);
  });

  let left = false;
  const enter = () => {
    if (left) return;
    left = true;
    window.clearInterval(polling);
    pulse.innerHTML = "<i></i> Rakip oturdu";
    veil.hidden = false;
    window.setTimeout(() => goPlay(seat), reducedWait() ? 80 : 720);
  };

  const watch = async () => {
    const match = await api(`/api/matches/${seat.MatchId}`);
    if (match.Status !== "Waiting") enter();
  };

  polling = window.setInterval(() => watch().catch((error) => {
    window.clearInterval(polling);
    note.textContent = error.message || "Masa cevap vermedi.";
  }), 1200);
  watch().catch(() => {});

  if (window.signalR) {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/matches")
      .withAutomaticReconnect()
      .build();
    connection.on("state", (next) => {
      const match = normalize(next);
      if (match.Status && match.Status !== "Waiting") enter();
    });
    connection.start()
      .then(() => connection.invoke("JoinMatch", seat.MatchId))
      .catch(() => {});
  }
}

function stirTea() {
  if (reducedWait()) return;
  const clip = stirTea.clip || (stirTea.clip = new Audio("cay-karistir.mp3"));
  clip.currentTime = 0;
  clip.volume = 0.9;
  const playing = clip.play();
  if (!playing) return;
  playing.catch(() => {});
  window.clearTimeout(stirTea.stop);
  stirTea.stop = window.setTimeout(() => {
    clip.pause();
    clip.currentTime = 0;
  }, 2200);
}

function reducedWait() {
  return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

function cancelQueue() {
  window.clearInterval(polling);
  const seat = offered;
  offered = null;
  setBusy(false);
  note.textContent = "Arama bırakıldı.";
  dialog?.classList.remove("searching");
  if (dialog?.open) dialog.close();
  if (!seat?.MatchId) return;
  fetch(`/api/matches/queue/${seat.MatchId}`, { method: "DELETE" }).catch(() => {});
}

function fail(error) {
  window.clearInterval(polling);
  setBusy(false);
  dialog?.classList.remove("searching");
  if (dialog?.open) dialog.close();
  note.textContent = error.message || "Masa açılamadı.";
}

function setBusy(busy) {
  if (findButton) findButton.disabled = busy;
  if (botButton) botButton.disabled = busy;
  if (hostButton) hostButton.disabled = busy;
  joinForm?.querySelectorAll("button, input").forEach((control) => { control.disabled = busy; });
}

const ACCOUNT_KEY = "kiraathane-account";

function readAccount() {
  try {
    const account = JSON.parse(localStorage.getItem(ACCOUNT_KEY) || "null");
    if (!account?.AccessToken || !account.ExpiresAt) return null;
    if (Date.parse(account.ExpiresAt) <= Date.now()) {
      localStorage.removeItem(ACCOUNT_KEY);
      return null;
    }
    return account;
  } catch {
    return null;
  }
}

function paintAccount() {
  const account = readAccount();
  const label = account ? (account.FullName || account.RegisteredName) : "Kapı";
  const onDoor = location.pathname.endsWith("/kapi.html");
  const nav = document.querySelector(".desk-nav");
  if (nav && !nav.querySelector("[data-account]")) {
    const link = document.createElement("a");
    link.dataset.account = "true";
    link.href = "/kapi.html";
    link.className = "account-chip";
    nav.append(link);
  }
  document.querySelectorAll(".dock").forEach((dock) => {
    if (dock.querySelector("[data-profile]")) return;
    const link = document.createElement("a");
    link.dataset.profile = "true";
    link.href = "/kapi.html";
    link.innerHTML = '<svg class="ico" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="3.2"/><path d="M6.2 19.2c.8-3 2.8-4.6 5.8-4.6s5 1.6 5.8 4.6"/></svg><span></span>';
    if (onDoor) link.setAttribute("aria-current", "page");
    dock.append(link);
  });
  document.querySelectorAll("[data-account], [data-profile] span").forEach((node) => {
    node.textContent = label;
  });
}

const RANKS = [
  [0, "Çırak"],
  [3, "Kalfa"],
  [8, "Usta"],
  [15, "Kıraathane Ağası"]
];

function winsKey(account) {
  return `kiraathane-wins:${account?.UserId || "misafir"}`;
}

function readWins(account = readAccount()) {
  const raw = Number(localStorage.getItem(winsKey(account)));
  return Number.isFinite(raw) && raw > 0 ? Math.floor(raw) : 0;
}

function rankFor(wins) {
  let title = RANKS[0][1];
  let next = RANKS[1][0];
  for (let index = 0; index < RANKS.length; index += 1) {
    if (wins >= RANKS[index][0]) {
      title = RANKS[index][1];
      next = RANKS[index + 1]?.[0] ?? null;
    }
  }
  return { title, next };
}

function noteWin(matchId) {
  const account = readAccount();
  if (!account || !matchId) return readWins(account);
  const seenKey = `kiraathane-wins-seen:${account.UserId}`;
  const seen = JSON.parse(localStorage.getItem(seenKey) || "[]");
  if (seen.includes(matchId)) return readWins(account);
  const wins = readWins(account) + 1;
  localStorage.setItem(winsKey(account), String(wins));
  localStorage.setItem(seenKey, JSON.stringify([matchId, ...seen].slice(0, 40)));
  return wins;
}

window.KiraathaneAccount = { read: readAccount, paint: paintAccount, wins: readWins, rank: rankFor, noteWin };

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
    throw new Error(errors?.[0]?.message || errors?.[0]?.Message || body.message || body.Message || "Masa cevap vermedi.");
  }
  return normalize(body.data ?? body.Data);
}

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

function readBoard() {
  try { return JSON.parse(localStorage.getItem(BOARD_KEY) || "[]"); }
  catch { return []; }
}
function escapeHtml(value) {
  return String(value).replace(/[&<>"']/g, (char) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;"
  })[char]);
}

window.kiraathaneWall = {
  record(entry) {
    const rows = readBoard().filter((row) => row.matchId !== entry.matchId);
    rows.unshift(entry);
    localStorage.setItem(BOARD_KEY, JSON.stringify(rows.slice(0, 12)));
  }
};
