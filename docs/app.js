const playButton = document.querySelector("#playButton");
const gameFrame = document.querySelector("#gameFrame");
const screenOverlay = document.querySelector("#screenOverlay");
const gameStatus = document.querySelector("#gameStatus");

async function hasGameBuild(url) {
  try {
    const response = await fetch(url, { method: "HEAD", cache: "no-store" });
    return response.ok;
  } catch {
    return false;
  }
}

async function loadGame() {
  const gameUrl = gameFrame.dataset.src;
  playButton.disabled = true;
  gameStatus.textContent = "Checking for a web build...";

  if (await hasGameBuild(gameUrl)) {
    gameFrame.src = gameUrl;
    screenOverlay.classList.add("is-hidden");
    return;
  }

  playButton.disabled = false;
  gameStatus.innerHTML =
    "No browser build found yet. Put one at <code>docs/game/index.html</code>, then deploy this folder.";
}

playButton.addEventListener("click", loadGame);
