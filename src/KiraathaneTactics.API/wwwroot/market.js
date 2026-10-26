(() => {
const accountApi = window.KiraathaneAccount;
const purseNode = document.querySelector("#market-purse strong");
const note = document.querySelector("#market-note");
const grid = document.querySelector("#market-grid");

paint();

function paint() {
  const account = accountApi.read();
  const purse = accountApi.purse(account);
  purseNode.textContent = String(purse.gold);
  grid.replaceChildren();
  if (!account) {
    note.textContent = "Tezgâh kapıdan girenlere açık.";
    return;
  }
  note.textContent = "";
  ["faces", "frames", "colors"].forEach((kind) => {
    accountApi.looks[kind].forEach((item) => {
      const slot = kind === "faces" ? "face" : kind === "frames" ? "frame" : "color";
      const token = `${slot}:${item.id}`;
      const owned = purse.owned.includes(token);
      const selected = purse[slot] === item.id;
      const preview = {
        ...purse,
        frame: kind === "frames" ? item.id : purse.frame,
        color: kind === "colors" ? item.id : purse.color,
        face: kind === "faces" ? item.id : purse.face
      };
      const card = document.createElement("article");
      card.className = "market-card";
      const swatch = document.createElement("span");
      swatch.className = `door-avatar frame-${preview.frame}`;
      swatch.style.cssText = accountApi.avatarStyle(preview);
      accountApi.mount(swatch, preview, 4);
      const copy = document.createElement("p");
      copy.innerHTML = `<strong>${item.name}</strong><br><small>${item.price ? `${item.price} altın` : "Bedava"}</small>`;
      const button = document.createElement("button");
      button.type = "button";
      button.textContent = selected ? "Takılı" : owned ? "Tak" : "Al";
      button.disabled = selected;
      button.addEventListener("click", () => {
        try {
          accountApi.buy(kind, item.id);
          paint();
        } catch (error) {
          note.textContent = error.message;
        }
      });
      card.append(swatch, copy, button);
      grid.append(card);
    });
  });
}
})();
