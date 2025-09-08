document.addEventListener('DOMContentLoaded', function () {
    console.log("Page loaded successfully!");
    document.querySelector('h1').addEventListener('click', function () {
        alert('Hello from middleware!');
    });
});