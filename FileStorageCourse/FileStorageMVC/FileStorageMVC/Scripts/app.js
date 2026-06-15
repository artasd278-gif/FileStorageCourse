(() => {
  const navbar = document.querySelector(".app-navbar");
  if (!navbar) return;

  const onScroll = () => {
    if (window.scrollY > 8) {
      navbar.classList.add("shadow-sm");
    } else {
      navbar.classList.remove("shadow-sm");
    }
  };

  onScroll();
  window.addEventListener("scroll", onScroll);
})();
