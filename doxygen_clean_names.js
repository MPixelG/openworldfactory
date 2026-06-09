document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".memname").forEach(el => {
        const full = el.textContent.trim();

        el.setAttribute("title", full);

        const parts = full.split(".");

        if (parts.length > 1) {
            const shortName = parts[parts.length - 1];
            el.textContent = shortName;
        }
    });
});