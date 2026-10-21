const note = document.querySelector("#queue-note");
const dialog = document.querySelector("#queue");
const queueCopy = document.querySelector("#queue-copy");
const queueCode = document.querySelector("#queue-code");
const findButton = document.querySelector("#find-rival");
const botButton = document.querySelector("#play-bot");
const board = document.querySelector("#leaderboard");

const QUEUE_KEY = "kiraathane-open-table";
const BOARD_KEY = "kiraathane-wall";
let polling = null;

renderBoard();
window.addEventListener("storage", (event) => {
  if (event.key === QUEUE_KEY) claimWaitingTable();
});

findButton.addEventListener("click", () => findRival().catch(fail));
botButton.addEventListener("click", () => playBot().catch(fail));
document.querySelector("#queue-cancel").addEventListener("click", cancelQueue);

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
  const waiting = readQueue();
  if (waiting && Date.now() - waiting.openedAt < 90_000) {
    setBusy(true);
    note.textContent = "Boş masa bulundu, oturuluyor.";
    const seat = await api("/api/matches/rooms/join", {
      method: "POST",
      body: JSON.stringify({ roomCode: waiting.roomCode })
    });
    clearQueue();
    goPlay(seat);
    return;
  }

  setBusy(true);
  const seat = await api("/api/matches/rooms", { method: "POST" });
  writeQueue({ roomCode: seat.RoomCode, matchId: seat.MatchId, openedAt: Date.now() });
  queueCopy.textContent = "Rakip aranıyor. Başka bir sekme ya da telefon aynı düğmeye basarsa masaya oturur.";
  queueCode.hidden = false;
  queueCode.textContent = seat.RoomCode;
  if (typeof dialog.showModal === "function") dialog.showModal();
  polling = window.setInterval(() => watchTable(seat).catch(fail), 2000);
}

async function watchTable(seat) {
  const match = await api(`/api/matches/${seat.MatchId}`);
  if (match.Status === "Waiting") return;
  window.clearInterval(polling);
  clearQueue();
  dialog.close();
  goPlay(seat);
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
  window.location.href = `/play/index.html?v=14&from=landing${bot}`;
}

function cancelQueue() {
  window.clearInterval(polling);
  clearQueue();
  setBusy(false);
  note.textContent = "Arama bırakıldı.";
}

function claimWaitingTable() {
  if (dialog.open) return;
  const waiting = readQueue();
  if (!waiting) return;
  note.textContent = `${waiting.roomCode} masası hâlâ boş.`;
}

function fail(error) {
  window.clearInterval(polling);
  setBusy(false);
  if (dialog.open) dialog.close();
  note.textContent = error.message || "Masa açılamadı.";
}

function setBusy(busy) {
  findButton.disabled = busy;
  botButton.disabled = busy;
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

function readQueue() {
  try { return JSON.parse(localStorage.getItem(QUEUE_KEY) || "null"); }
  catch { return null; }
}
function writeQueue(value) { localStorage.setItem(QUEUE_KEY, JSON.stringify(value)); }
function clearQueue() { localStorage.removeItem(QUEUE_KEY); }
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
