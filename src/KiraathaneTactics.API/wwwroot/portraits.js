/* Kıraathane yüzleri. Daireye sığan, tanınır büstler. */
window.paintPortrait = function paintPortrait(canvas, id) {
  const face = FACES[id] || FACES.dayi;
  canvas.width = 160;
  canvas.height = 160;
  const pen = canvas.getContext("2d");
  pen.clearRect(0, 0, 160, 160);
  pen.save();
  pen.beginPath();
  pen.arc(80, 80, 78, 0, Math.PI * 2);
  pen.clip();
  face(pen);
  pen.restore();
};

const FACES = {
  dayi(pen) {
    wash(pen, "#c4a07a", "#4a2c1a");
    shoulders(pen, "#f4efe4", "#cbbba4");
    neck(pen);
    collar(pen, "#efe6d4");
    tie(pen, "#6b1d28");
    hair(pen, "#1a120e");
    head(pen);
    brow(pen, "#1a120e", 3.2, true);
    eye(pen, 64, 86);
    eye(pen, 96, 86);
    glasses(pen);
    nose(pen);
    mustache(pen, "#3a2418", 28);
    mouth(pen, 114);
    hat(pen, "#241c18");
    band(pen, "#c9a15b");
  },
  cayci(pen) {
    wash(pen, "#e7c8a4", "#6a3a28");
    shoulders(pen, "#9d1c2e", "#6e101c");
    neck(pen);
    apron(pen);
    hair(pen, "#24160f");
    head(pen);
    brow(pen, "#24160f", 2.4, false);
    eye(pen, 64, 88, true);
    eye(pen, 96, 88, true);
    nose(pen);
    mustache(pen, "#5c331f", 16);
    smile(pen, 116);
    cap(pen, "#f7f1e4");
  },
  usta(pen) {
    wash(pen, "#d7c2a4", "#3a2a20");
    shoulders(pen, "#2c241c", "#16110e");
    neck(pen);
    vest(pen, "#c9a15b");
    hair(pen, "#1a120e");
    head(pen);
    brow(pen, "#1a120e", 3.4, true);
    eye(pen, 64, 88);
    eye(pen, 96, 88);
    nose(pen);
    mustache(pen, "#2a1a12", 24);
    mouth(pen, 116);
    flatCap(pen, "#e2b85a", "#6b4a22");
  },
  ogrenci(pen) {
    wash(pen, "#d7e0f2", "#24386a");
    shoulders(pen, "#24386a", "#182848");
    neck(pen);
    shirt(pen, "#f7f4ee");
    hair(pen, "#14203a");
    head(pen);
    fringe(pen);
    brow(pen, "#14203a", 2, false);
    eye(pen, 64, 88, true);
    eye(pen, 96, 88, true);
    nose(pen);
    smile(pen, 114);
    blush(pen);
  },
  hanim(pen) {
    wash(pen, "#f6e4d4", "#7a3044");
    shoulders(pen, "#9d1c2e", "#6e1424");
    neck(pen);
    neckline(pen);
    hair(pen, "#2a1218");
    bun(pen);
    head(pen);
    brow(pen, "#2a1218", 1.8, false);
    eye(pen, 64, 86, true);
    eye(pen, 96, 86, true);
    lash(pen, 64, 82);
    lash(pen, 96, 82);
    nose(pen);
    lip(pen, 112);
    blush(pen);
    earring(pen, 46, 98);
    earring(pen, 114, 98);
  },
  satranc(pen) {
    wash(pen, "#e8e4da", "#1c2444");
    shoulders(pen, "#f7f4ee", "#cfc8b8");
    neck(pen);
    vest(pen, "#1f6b45");
    collar(pen, "#f7f4ee");
    bow(pen);
    hair(pen, "#16120e");
    head(pen);
    sidePart(pen);
    brow(pen, "#16120e", 2.2, false);
    eye(pen, 64, 86);
    eye(pen, 96, 86);
    roundGlasses(pen);
    nose(pen);
    mouth(pen, 114);
  },
  nargile(pen) {
    wash(pen, "#e7d3b0", "#5a3418");
    shoulders(pen, "#6b2a1a", "#3a140e");
    neck(pen);
    scarf(pen, "#c9a15b");
    hair(pen, "#1a100c");
    head(pen);
    brow(pen, "#1a100c", 2.6, true);
    eye(pen, 64, 88);
    eye(pen, 96, 88);
    nose(pen);
    beard(pen, "#2a1a12");
    fez(pen);
    hose(pen);
  },
  sair(pen) {
    wash(pen, "#efe4d4", "#4a3048");
    shoulders(pen, "#2a2438", "#161222");
    neck(pen);
    cravat(pen);
    hair(pen, "#241820");
    head(pen);
    wave(pen);
    brow(pen, "#241820", 2, false);
    eye(pen, 64, 86);
    eye(pen, 96, 86);
    nose(pen);
    mustache(pen, "#3a241c", 18);
    mouth(pen, 116);
  },
  gazeteci(pen) {
    wash(pen, "#d7e2ea", "#243848");
    shoulders(pen, "#d8d2c4", "#8a8474");
    neck(pen);
    collar(pen, "#f7f4ee");
    tie(pen, "#1c3a5a");
    hair(pen, "#2a241c");
    head(pen);
    brow(pen, "#2a241c", 2.4, true);
    eye(pen, 64, 86);
    eye(pen, 96, 86);
    nose(pen);
    mouth(pen, 114);
    pressHat(pen);
    card(pen);
  },
  muavin(pen) {
    wash(pen, "#dce6d4", "#2a4030");
    shoulders(pen, "#1f4a34", "#10281c");
    neck(pen);
    collar(pen, "#f4efe4");
    hair(pen, "#1a140e");
    head(pen);
    brow(pen, "#1a140e", 2.8, true);
    eye(pen, 64, 88);
    eye(pen, 96, 88);
    nose(pen);
    mustache(pen, "#24160f", 20);
    mouth(pen, 116);
    kepi(pen);
  }
};

function wash(pen, top, bottom) {
  const sky = pen.createLinearGradient(0, 0, 0, 160);
  sky.addColorStop(0, top);
  sky.addColorStop(1, bottom);
  pen.fillStyle = sky;
  pen.fillRect(0, 0, 160, 160);
}

function head(pen) {
  const skin = pen.createRadialGradient(68, 70, 8, 80, 92, 48);
  skin.addColorStop(0, "#ffe7cf");
  skin.addColorStop(0.7, "#efc7a2");
  skin.addColorStop(1, "#d7a27c");
  pen.fillStyle = skin;
  pen.beginPath();
  pen.ellipse(80, 92, 32, 36, 0, 0, Math.PI * 2);
  pen.fill();
  pen.fillStyle = "rgba(90, 42, 24, 0.14)";
  pen.beginPath();
  pen.ellipse(80, 118, 14, 6, 0, 0, Math.PI);
  pen.fill();
}

function neck(pen) {
  pen.fillStyle = "#e7b892";
  pen.beginPath();
  pen.moveTo(70, 112);
  pen.lineTo(90, 112);
  pen.lineTo(94, 136);
  pen.lineTo(66, 136);
  pen.closePath();
  pen.fill();
}

function shoulders(pen, cloth, shade) {
  const coat = pen.createLinearGradient(0, 100, 0, 160);
  coat.addColorStop(0, cloth);
  coat.addColorStop(1, shade);
  pen.fillStyle = coat;
  pen.beginPath();
  pen.moveTo(8, 160);
  pen.quadraticCurveTo(24, 112, 80, 108);
  pen.quadraticCurveTo(136, 112, 152, 160);
  pen.closePath();
  pen.fill();
}

function hair(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.ellipse(80, 70, 34, 24, 0, Math.PI * 1.02, Math.PI * 1.98);
  pen.quadraticCurveTo(46, 96, 50, 108);
  pen.quadraticCurveTo(62, 86, 80, 90);
  pen.quadraticCurveTo(98, 86, 110, 108);
  pen.quadraticCurveTo(114, 96, 112, 70);
  pen.fill();
}

function hat(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(58, 62);
  pen.quadraticCurveTo(80, 28, 104, 58);
  pen.lineTo(100, 66);
  pen.quadraticCurveTo(80, 42, 62, 68);
  pen.closePath();
  pen.fill();
  pen.beginPath();
  pen.ellipse(80, 66, 48, 7, 0, 0, Math.PI * 2);
  pen.fill();
}

function band(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.ellipse(80, 60, 22, 4, 0, 0, Math.PI * 2);
  pen.fill();
}

function cap(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.ellipse(78, 58, 30, 16, 0, Math.PI, 0);
  pen.fill();
  pen.beginPath();
  pen.ellipse(82, 60, 38, 6, 0.08, 0, Math.PI * 2);
  pen.fill();
  pen.fillStyle = "#c9a15b";
  pen.fillRect(58, 56, 40, 4);
}

function flatCap(pen, cloth, shade) {
  pen.fillStyle = shade;
  pen.beginPath();
  pen.ellipse(76, 56, 32, 14, -0.15, Math.PI, 0);
  pen.fill();
  pen.fillStyle = cloth;
  pen.beginPath();
  pen.ellipse(84, 62, 40, 7, -0.08, 0, Math.PI * 2);
  pen.fill();
}

function fringe(pen) {
  pen.fillStyle = "#14203a";
  pen.beginPath();
  pen.moveTo(50, 72);
  pen.quadraticCurveTo(64, 96, 80, 78);
  pen.quadraticCurveTo(96, 98, 110, 70);
  pen.quadraticCurveTo(96, 60, 80, 66);
  pen.quadraticCurveTo(64, 60, 50, 72);
  pen.fill();
}

function bun(pen) {
  pen.fillStyle = "#2a1218";
  pen.beginPath();
  pen.arc(108, 62, 13, 0, Math.PI * 2);
  pen.fill();
}

function sidePart(pen) {
  pen.strokeStyle = "rgba(247, 244, 238, 0.75)";
  pen.lineWidth = 1.6;
  pen.beginPath();
  pen.moveTo(70, 58);
  pen.quadraticCurveTo(76, 74, 72, 88);
  pen.stroke();
}

function collar(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(68, 118);
  pen.lineTo(80, 138);
  pen.lineTo(62, 134);
  pen.closePath();
  pen.moveTo(92, 118);
  pen.lineTo(80, 138);
  pen.lineTo(98, 134);
  pen.closePath();
  pen.fill();
}

function tie(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(80, 124);
  pen.lineTo(88, 134);
  pen.lineTo(80, 158);
  pen.lineTo(72, 134);
  pen.closePath();
  pen.fill();
}

function apron(pen) {
  pen.fillStyle = "#f7f1e4";
  pen.beginPath();
  pen.moveTo(64, 124);
  pen.lineTo(96, 124);
  pen.lineTo(104, 160);
  pen.lineTo(56, 160);
  pen.closePath();
  pen.fill();
  pen.strokeStyle = "#c9a15b";
  pen.lineWidth = 3;
  pen.beginPath();
  pen.moveTo(80, 112);
  pen.lineTo(80, 128);
  pen.moveTo(58, 118);
  pen.quadraticCurveTo(80, 130, 102, 118);
  pen.stroke();
}

function vest(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(66, 122);
  pen.lineTo(80, 134);
  pen.lineTo(94, 122);
  pen.lineTo(102, 160);
  pen.lineTo(58, 160);
  pen.closePath();
  pen.fill();
}

function shirt(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(70, 120);
  pen.lineTo(90, 120);
  pen.lineTo(86, 156);
  pen.lineTo(74, 156);
  pen.closePath();
  pen.fill();
}

function neckline(pen) {
  pen.strokeStyle = "#f2d7c4";
  pen.lineWidth = 3;
  pen.beginPath();
  pen.moveTo(66, 124);
  pen.quadraticCurveTo(80, 142, 94, 124);
  pen.stroke();
}

function earring(pen, x, y) {
  pen.fillStyle = "#e8c56a";
  pen.beginPath();
  pen.arc(x, y, 3.5, 0, Math.PI * 2);
  pen.fill();
}

function brow(pen, color, weight, heavy) {
  pen.strokeStyle = color;
  pen.lineWidth = weight;
  pen.lineCap = "round";
  const lift = heavy ? 78 : 80;
  pen.beginPath();
  pen.moveTo(54, lift + 2);
  pen.quadraticCurveTo(64, lift - (heavy ? 6 : 3), 74, lift);
  pen.moveTo(86, lift);
  pen.quadraticCurveTo(96, lift - (heavy ? 6 : 3), 106, lift + 2);
  pen.stroke();
}

function eye(pen, x, y, bright) {
  pen.fillStyle = "#fffaf3";
  pen.beginPath();
  pen.ellipse(x, y, 7, 5, 0, 0, Math.PI * 2);
  pen.fill();
  pen.fillStyle = bright ? "#3d6b4f" : "#2a211c";
  pen.beginPath();
  pen.arc(x + 1, y, 2.6, 0, Math.PI * 2);
  pen.fill();
  pen.fillStyle = "#fff";
  pen.beginPath();
  pen.arc(x + 2, y - 1, 1, 0, Math.PI * 2);
  pen.fill();
}

function lash(pen, x, y) {
  pen.strokeStyle = "#2a1218";
  pen.lineWidth = 1.5;
  pen.beginPath();
  pen.moveTo(x - 6, y);
  pen.quadraticCurveTo(x, y - 4, x + 6, y);
  pen.stroke();
}

function glasses(pen) {
  pen.strokeStyle = "#1a120e";
  pen.lineWidth = 2.4;
  pen.beginPath();
  pen.roundRect(52, 80, 24, 14, 4);
  pen.roundRect(84, 80, 24, 14, 4);
  pen.moveTo(76, 87);
  pen.lineTo(84, 87);
  pen.stroke();
}

function roundGlasses(pen) {
  pen.strokeStyle = "#c9a15b";
  pen.lineWidth = 2.2;
  pen.beginPath();
  pen.arc(64, 86, 10, 0, Math.PI * 2);
  pen.arc(96, 86, 10, 0, Math.PI * 2);
  pen.moveTo(74, 86);
  pen.lineTo(86, 86);
  pen.stroke();
}

function nose(pen) {
  pen.strokeStyle = "rgba(120, 62, 36, 0.75)";
  pen.lineWidth = 2;
  pen.lineCap = "round";
  pen.beginPath();
  pen.moveTo(80, 90);
  pen.quadraticCurveTo(86, 100, 80, 104);
  pen.stroke();
}

function mustache(pen, color, width) {
  const x = 80;
  const y = 108;
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(x - width, y);
  pen.quadraticCurveTo(x - width / 2, y - 8, x, y - 1);
  pen.quadraticCurveTo(x + width / 2, y - 8, x + width, y);
  pen.quadraticCurveTo(x + width / 2, y + 7, x, y + 2);
  pen.quadraticCurveTo(x - width / 2, y + 7, x - width, y);
  pen.fill();
}

function mouth(pen, y) {
  pen.strokeStyle = "#8a4d3a";
  pen.lineWidth = 2;
  pen.lineCap = "round";
  pen.beginPath();
  pen.moveTo(73, y);
  pen.quadraticCurveTo(80, y + 3, 87, y);
  pen.stroke();
}

function smile(pen, y) {
  pen.strokeStyle = "#8a4d3a";
  pen.lineWidth = 2.2;
  pen.lineCap = "round";
  pen.beginPath();
  pen.moveTo(72, y);
  pen.quadraticCurveTo(80, y + 8, 88, y);
  pen.stroke();
}

function lip(pen, y) {
  pen.fillStyle = "#b00018";
  pen.beginPath();
  pen.moveTo(72, y);
  pen.quadraticCurveTo(80, y - 4, 88, y);
  pen.quadraticCurveTo(80, y + 6, 72, y);
  pen.fill();
}

function blush(pen) {
  pen.fillStyle = "rgba(196, 85, 58, 0.28)";
  pen.beginPath();
  pen.ellipse(58, 100, 6, 3.5, 0, 0, Math.PI * 2);
  pen.ellipse(102, 100, 6, 3.5, 0, 0, Math.PI * 2);
  pen.fill();
}

function bow(pen) {
  pen.fillStyle = "#1c2444";
  pen.beginPath();
  pen.moveTo(80, 132);
  pen.lineTo(68, 124);
  pen.lineTo(68, 140);
  pen.closePath();
  pen.moveTo(80, 132);
  pen.lineTo(92, 124);
  pen.lineTo(92, 140);
  pen.closePath();
  pen.fill();
  pen.beginPath();
  pen.arc(80, 132, 3, 0, Math.PI * 2);
  pen.fill();
}

function scarf(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(62, 118);
  pen.quadraticCurveTo(80, 132, 98, 118);
  pen.lineTo(104, 148);
  pen.lineTo(88, 142);
  pen.lineTo(80, 158);
  pen.lineTo(72, 140);
  pen.lineTo(56, 146);
  pen.closePath();
  pen.fill();
}

function beard(pen, color) {
  pen.fillStyle = color;
  pen.beginPath();
  pen.moveTo(58, 104);
  pen.quadraticCurveTo(62, 128, 80, 132);
  pen.quadraticCurveTo(98, 128, 102, 104);
  pen.quadraticCurveTo(90, 112, 80, 110);
  pen.quadraticCurveTo(70, 112, 58, 104);
  pen.fill();
}

function fez(pen) {
  pen.fillStyle = "#9d1c2e";
  pen.beginPath();
  pen.moveTo(56, 62);
  pen.lineTo(104, 62);
  pen.lineTo(100, 46);
  pen.lineTo(60, 46);
  pen.closePath();
  pen.fill();
  pen.fillStyle = "#1a100c";
  pen.fillRect(58, 60, 44, 5);
  pen.strokeStyle = "#1a100c";
  pen.lineWidth = 1.4;
  pen.beginPath();
  pen.moveTo(80, 46);
  pen.quadraticCurveTo(86, 34, 92, 28);
  pen.stroke();
}

function hose(pen) {
  pen.strokeStyle = "#6b2a1a";
  pen.lineWidth = 3;
  pen.lineCap = "round";
  pen.beginPath();
  pen.moveTo(112, 108);
  pen.quadraticCurveTo(128, 96, 124, 78);
  pen.stroke();
  pen.fillStyle = "#c9a15b";
  pen.beginPath();
  pen.arc(124, 74, 5, 0, Math.PI * 2);
  pen.fill();
}

function wave(pen) {
  pen.fillStyle = "#241820";
  pen.beginPath();
  pen.moveTo(48, 78);
  pen.quadraticCurveTo(58, 96, 72, 82);
  pen.quadraticCurveTo(64, 68, 52, 70);
  pen.closePath();
  pen.fill();
}

function cravat(pen) {
  pen.fillStyle = "#7a3044";
  pen.beginPath();
  pen.moveTo(80, 122);
  pen.lineTo(92, 136);
  pen.lineTo(80, 156);
  pen.lineTo(68, 136);
  pen.closePath();
  pen.fill();
  pen.fillStyle = "#f4efe4";
  pen.beginPath();
  pen.arc(80, 128, 3, 0, Math.PI * 2);
  pen.fill();
}

function pressHat(pen) {
  pen.fillStyle = "#f4efe4";
  pen.beginPath();
  pen.moveTo(54, 58);
  pen.lineTo(108, 54);
  pen.lineTo(104, 42);
  pen.lineTo(58, 44);
  pen.closePath();
  pen.fill();
  pen.fillStyle = "#1c3a5a";
  pen.fillRect(60, 50, 44, 5);
  pen.beginPath();
  pen.ellipse(82, 58, 42, 5, -0.06, 0, Math.PI * 2);
  pen.fill();
}

function card(pen) {
  pen.fillStyle = "#f7f4ee";
  pen.save();
  pen.translate(118, 118);
  pen.rotate(0.4);
  pen.fillRect(0, 0, 22, 16);
  pen.fillStyle = "#1c3a5a";
  pen.fillRect(3, 4, 12, 2);
  pen.fillRect(3, 8, 16, 1.4);
  pen.restore();
}

function kepi(pen) {
  pen.fillStyle = "#1f4a34";
  pen.beginPath();
  pen.moveTo(58, 60);
  pen.lineTo(102, 60);
  pen.lineTo(96, 44);
  pen.lineTo(64, 44);
  pen.closePath();
  pen.fill();
  pen.fillStyle = "#c9a15b";
  pen.fillRect(64, 50, 32, 3);
  pen.fillStyle = "#163326";
  pen.beginPath();
  pen.ellipse(86, 62, 36, 6, 0.1, 0, Math.PI * 2);
  pen.fill();
}
