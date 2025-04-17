window.pdfScrollHelper = {
    detectScrollEnd: function (dotNetHelper) {
        const element = document.getElementById('pdfViewer');
        if (!element) {
            console.error("❌ pdfViewer element not found");
            return;
        }

        console.log("📜 PDF viewer element found, adding scroll listener...");

        // Remove existing scroll listeners
        const newElement = element.cloneNode(true);
        element.parentNode.replaceChild(newElement, element);

        newElement.addEventListener('scroll', function () {
            const scrollTop = newElement.scrollTop;
            const scrollHeight = newElement.scrollHeight;
            const clientHeight = newElement.clientHeight;

            console.log("📜 Scrolling...");
            console.log("scrollTop:", scrollTop);
            console.log("clientHeight:", clientHeight);
            console.log("scrollHeight:", scrollHeight);

            if (scrollTop + clientHeight >= scrollHeight - 10) {
                console.log("✅ Reached bottom of PDF viewer");
                dotNetHelper.invokeMethodAsync('OnPdfScrolledToEnd')
                    .then(() => console.log("🟢 Method invoked successfully"))
                    .catch(err => console.error("🚨 Error invoking method:", err));
            }
        });
    }
};