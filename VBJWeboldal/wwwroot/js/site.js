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


    const esemenyekBtn = document.getElementById("esemenyekBtn");
    console.log("nem jo elott")

    esemenyekBtn.addEventListener("click", function () {
        console.log("nem jo utan")
        const target = document.getElementById("kozelgoEsemenyek");
        if (!target) return;

        target.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

        // animáció újraindítás
        target.classList.remove("highlight-flash");
        void target.offsetWidth;
        target.classList.add("highlight-flash");
    });
    hirekBtn.addEventListener("click", function () {
        console.log("nem jo utan")
        const target = document.getElementById("hirek");
        if (!target) return;

        target.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

        // animáció újraindítás
        target.classList.remove("highlight-flash");
        void target.offsetWidth;
        target.classList.add("highlight-flash");
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
