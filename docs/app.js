const playButton = document.querySelector("#playButton");
const gameFrame = document.querySelector("#gameFrame");
const screenOverlay = document.querySelector("#screenOverlay");
const gameStatus = document.querySelector("#gameStatus");

async function hasGameBuild(url) {
  try {
    const response = await fetch(url, { method: "HEAD", cache: "no-store" });
    return response.ok && response.headers.get("content-type")?.includes("text/html");
  } catch {
    return false;
  }
}

async function loadGame() {
  const gameUrl = gameFrame.dataset.src;
  playButton.disabled = true;
  gameStatus.textContent = "Loading the game...";

  if (await hasGameBuild(gameUrl)) {
    gameFrame.src = gameUrl;
    screenOverlay.classList.add("is-hidden");
    gameFrame.focus();
    return;
  }

  playButton.disabled = false;
  gameStatus.textContent = "The game could not be loaded. Please try again in a moment.";
}

gameFrame.addEventListener("load", () => {
  if (gameFrame.hasAttribute("src")) {
    gameFrame.contentWindow.focus();
  }
});

playButton.addEventListener("click", loadGame);
