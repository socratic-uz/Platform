document.addEventListener('keydown', function (e) {
	if (e.key === 'F12' ||
		(e.ctrlKey && e.shiftKey && e.key === 'I') ||
		(e.ctrlKey && e.shiftKey && e.key === 'J') ||
		(e.ctrlKey && e.shiftKey && e.key === 'C') ||
		(e.ctrlKey && e.key === 'U')) {
		e.preventDefault();
	}
});

(function () {
	const threshold = 160;
	setInterval(function () {
		if (window.outerWidth - window.innerWidth > threshold ||
			window.outerHeight - window.innerHeight > threshold) {
			document.body.innerHTML = `
		<div style="
		  display:grid;
		  place-items: center;
		  height: 100vh;
		  margin: 0;
		  font-family: sans-serif;">
		  <h1>DevTools запрещён</h1>
		</div>`;
		}
	}, 500);
})();