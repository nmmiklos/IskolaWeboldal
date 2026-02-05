document.addEventListener("DOMContentLoaded", function () {

    const y = document.getElementById('y');
    if (y) {
        y.textContent = new Date().getFullYear();
    }

    const btn = document.getElementById("menuBtn");
    const panel = document.getElementById("menuPanel");

    btn.addEventListener("click", (e) => {
        e.stopPropagation();
        panel.classList.toggle("open");
    });

    document.addEventListener("click", () => {
        panel.classList.remove("open");
    });


});
// DARK MODE TOGGLE
const toggle = document.getElementById("themeToggle");

toggle.addEventListener("click", () => {
    const isDark = document.body.classList.contains("dark");

    document.body.classList.toggle("dark", !isDark);
    localStorage.setItem("theme", !isDark ? "dark" : "light");
});
const savedTheme = localStorage.getItem("theme");

if (savedTheme === "dark") {
    document.body.classList.add("dark");
}


