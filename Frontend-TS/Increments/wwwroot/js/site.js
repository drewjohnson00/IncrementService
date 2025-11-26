// Example TypeScript entry - update for your app
export function greet(name) {
    const el = document.getElementById('greeting');
    if (el) {
        el.textContent = `Hello, ${name}!`;
    }
}
document.addEventListener('DOMContentLoaded', () => {
    greet('World');
});
//# sourceMappingURL=site.js.map