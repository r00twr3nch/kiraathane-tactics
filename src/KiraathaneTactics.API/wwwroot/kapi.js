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
  const nick = account.RegisteredName || account.FullName;
  const head = document.createElement("div");
  head.className = "door-id";
  const avatar = document.createElement("span");
  avatar.className = "door-avatar";
  avatar.setAttribute("aria-hidden", "true");
  avatar.textContent = initials(nick);
  const who = document.createElement("div");
  const name = document.createElement("strong");
  name.textContent = account.FullName;
  const mail = document.createElement("p");
  mail.className = "door-mail";
  mail.textContent = account.Email;
  who.append(name, mail);
  head.append(avatar, who);
  const facts = document.createElement("dl");
  facts.className = "door-facts";
  facts.append(
    fact("Kayıt nicki", nick, true),
    fact("Üyelik", since(account.MemberSince)),
    fact("Masa", account.Role === "Admin" ? "Müessese" : "Müdavim")
  );
  const form = document.createElement("form");
  const label = document.createElement("label");
  label.textContent = "Duvar adı";
  const field = document.createElement("input");
  field.name = "fullName";
  field.required = true;
  field.minLength = 2;
  field.maxLength = 24;
  field.autocomplete = "nickname";
  field.value = account.FullName;
  label.append(field);
  const save = document.createElement("button");
  save.className = "cta";
  save.type = "submit";
  save.textContent = "Adı kaydet";
  const status = document.createElement("p");
  status.className = "note";
  status.setAttribute("role", "status");
  form.append(label, save, status);
  form.addEventListener("submit", (event) => rename(event, status));
  const out = document.createElement("button");
  out.type = "button";
  out.textContent = "Çıkış";
  out.addEventListener("click", () => {
    localStorage.removeItem(ACCOUNT_KEY);
    if (window.KiraathaneAccount) window.KiraathaneAccount.paint();
    location.reload();
  });
  sessionBox.append(head, facts, form, out);
}

function initials(name) {
  const parts = String(name).trim().split(/\s+/).filter(Boolean);
  const letters = (parts[0]?.[0] || "") + (parts[1]?.[0] || "");
  return (letters || "K").toLocaleUpperCase("tr");
}

function since(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return date.toLocaleDateString("tr-TR", { day: "numeric", month: "long", year: "numeric" });
}

function fact(term, value, locked) {
  const wrap = document.createElement("div");
  const label = document.createElement("dt");
  label.textContent = term;
  const detail = document.createElement("dd");
  detail.textContent = value;
  if (locked) detail.dataset.locked = "true";
  wrap.append(label, detail);
  return wrap;
}

async function rename(event, status) {
  event.preventDefault();
  const form = event.currentTarget;
  const button = form.querySelector("button");
  button.disabled = true;
  status.classList.remove("error");
  status.textContent = "Defter düzeltiliyor…";
  try {
    const account = await api("/api/auth/profile", {
      method: "PATCH",
      body: JSON.stringify(Object.fromEntries(new FormData(form)))
    });
    localStorage.setItem(ACCOUNT_KEY, JSON.stringify(account));
    paintDoor();
    if (window.KiraathaneAccount) window.KiraathaneAccount.paint();
    const fresh = document.querySelector(".door-session .note");
    if (fresh) fresh.textContent = "Ad duvara işlendi.";
  } catch (error) {
    status.textContent = error.message;
    status.classList.add("error");
    button.disabled = false;
  }
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