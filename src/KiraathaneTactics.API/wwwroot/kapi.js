(() => {
const sessionBox = document.querySelector("#door-session");
const forms = document.querySelector("#door-forms");
const note = document.querySelector("#door-note");
const formIn = document.querySelector("#form-in");
const formUp = document.querySelector("#form-up");

document.querySelector("#tab-in").addEventListener("click", () => showTab("in"));
document.querySelector("#tab-up").addEventListener("click", () => showTab("up"));
formIn.addEventListener("submit", (event) => submitDoor(event, "/api/auth/login"));
formUp.addEventListener("submit", (event) => submitDoor(event, "/api/auth/register"));
paintDoor();
if (window.KiraathaneAccount) window.KiraathaneAccount.paint();

function showTab(which) {
  const entering = which === "in";
  formIn.hidden = !entering;
  formUp.hidden = entering;
  document.querySelector("#tab-in").setAttribute("aria-selected", String(entering));
  document.querySelector("#tab-up").setAttribute("aria-selected", String(!entering));
  note.textContent = "";
  note.classList.remove("error");
}

function paintDoor() {
  const account = readAccount();
  if (!account) {
    sessionBox.hidden = true;
    forms.hidden = false;
    return;
  }
  forms.hidden = true;
  sessionBox.hidden = false;
  sessionBox.className = "door-session";
  sessionBox.replaceChildren();
  const username = account.FullName || account.RegisteredName;
  const wins = window.KiraathaneAccount?.wins?.(account) ?? 0;
  const rank = window.KiraathaneAccount?.rank?.(wins) ?? { title: "Çırak", next: 3 };
  const avatar = document.createElement("span");
  avatar.className = "door-avatar";
  avatar.setAttribute("aria-hidden", "true");
  avatar.textContent = initials(username);
  const standing = document.createElement("p");
  standing.className = "door-rank";
  standing.textContent = rank.next == null
    ? `${rank.title} · ${wins} galibiyet`
    : `${rank.title} · ${wins} galibiyet · sıradaki ${rank.next}`;
  const facts = document.createElement("dl");
  facts.className = "door-lines";
  facts.append(line("İsim", username), line("Mail", account.Email));
  const out = document.createElement("button");
  out.type = "button";
  out.textContent = "Çıkış";
  out.addEventListener("click", () => {
    localStorage.removeItem(ACCOUNT_KEY);
    if (window.KiraathaneAccount) window.KiraathaneAccount.paint();
    location.reload();
  });
  sessionBox.append(avatar, standing, facts, out);
}

function line(term, value) {
  const wrap = document.createElement("div");
  const label = document.createElement("dt");
  label.textContent = term;
  const detail = document.createElement("dd");
  detail.textContent = value;
  wrap.append(label, detail);
  return wrap;
}

function initials(name) {
  const parts = String(name).trim().split(/\s+/).filter(Boolean);
  const letters = (parts[0]?.[0] || "") + (parts[1]?.[0] || "");
  return (letters || "K").toLocaleUpperCase("tr");
}

async function submitDoor(event, url) {
  event.preventDefault();
  const form = event.currentTarget;
  const data = Object.fromEntries(new FormData(form));
  const button = form.querySelector("button");
  button.disabled = true;
  note.classList.remove("error");
  note.textContent = "Kapı açılıyor…";
  try {
    const account = await api(url, { method: "POST", body: JSON.stringify(data) });
    localStorage.setItem(ACCOUNT_KEY, JSON.stringify(account));
    paintDoor();
    if (window.KiraathaneAccount) window.KiraathaneAccount.paint();
  } catch (error) {
    note.textContent = error.message;
    note.classList.add("error");
  } finally {
    button.disabled = false;
  }
}
})();