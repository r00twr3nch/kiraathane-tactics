const note = document.querySelector("#queue-note");
const dialog = document.querySelector("#queue");
const queueCopy = document.querySelector("#queue-copy");
const queueCode = document.querySelector("#queue-code");
const findButton = document.querySelector("#find-rival");
const botButton = document.querySelector("#play-bot");
const hostButton = document.querySelector("#host-room");
const hostCode = document.querySelector("#host-code");
const board = document.querySelector("#leaderboard");

const BOARD_KEY = "kiraathane-wall";
let polling = null;
let offered = null;

if (board) renderBoard();

findButton?.addEventListener("click", () => findRival().catch(fail));
botButton?.addEventListener("click", () => playBot().catch(fail));
hostButton?.addEventListener("click", () => hostRoom().catch(fail));
document.querySelector("#queue-cancel")?.addEventListener("click", cancelQueue);

function renderBoard() {
  const rows = readBoard();
  if (!rows.length) {
    board.innerHTML = `<li class="empty">Duvar boş. İlk maçı bitiren adını buraya yazar.</li>`;
    return;
  }
  board.replaceChildren(...rows.slice(0, 8).map((row, index) => {
    const item = document.createElement("li");
    item.innerHTML = `<span class="rank">${index + 1}</span><span><strong>${escapeHtml(row.name)}</strong><br />${escapeHtml(row.detail)}</span><span>${row.score}</span>`;
    return item;
  }));
}

async function findRival() {
  setBusy(true);
  note.textContent = "Boş masa aranıyor.";
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
    note.textContent = "Sıradasın. Rakip oturunca masa açılır.";
    queueCopy.textContent = "Rakip aranıyor. Başka bir telefon da aynı düğmeye basarsa masaya oturur.";
    queueCode.hidden = true;
    if (typeof dialog.showModal === "function") dialog.showModal();
    polling = window.setInterval(() => watchTable(seat).catch(fail), 1500);
    return;
  }

  note.textContent = "Boş masa bulundu, oturuluyor.";
  if (dialog.open) dialog.close();
  goPlay(seat);
}

async function watchTable(seat) {
  const match = await api(`/api/matches/${seat.MatchId}`);
  if (match.Status === "Waiting") return;
  window.clearInterval(polling);
  offered = null;
  if (dialog.open) dialog.close();
  goPlay(seat);
}

async function hostRoom() {
  setBusy(true);
  note.textContent = "Masa kuruluyor.";
  const seat = await api("/api/matches/rooms", { method: "POST" });
  hostCode.hidden = false;
  hostCode.textContent = seat.RoomCode;
  note.textContent = "Kod hazır. Karşıdaki oturunca masa açılır.";
  polling = window.setInterval(() => watchTable(seat).catch(fail), 2000);
}

async function playBot() {
  setBusy(true);
  note.textContent = "Çaycı önlüğü bağlıyor.";
  const light = await api("/api/matches/rooms?openingSide=Light", { method: "POST" });
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
  window.location.href = `/play/index.html?v=32&from=landing${bot}`;
}

function cancelQueue() {
  window.clearInterval(polling);
  const seat = offered;
  offered = null;
  setBusy(false);
  note.textContent = "Arama bırakıldı.";
  if (dialog.open) dialog.close();
  if (!seat?.MatchId) return;
  fetch(`/api/matches/queue/${seat.MatchId}`, { method: "DELETE" }).catch(() => {});
}

function fail(error) {
  window.clearInterval(polling);
  setBusy(false);
  if (dialog.open) dialog.close();
  note.textContent = error.message || "Masa açılamadı.";
}

function setBusy(busy) {
  if (findButton) findButton.disabled = busy;
  if (botButton) botButton.disabled = busy;
  if (hostButton) hostButton.disabled = busy;
}

async function api(url, options = {}) {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json", Accept: "application/json" },
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
