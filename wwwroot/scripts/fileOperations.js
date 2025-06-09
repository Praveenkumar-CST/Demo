// Function to trigger file input click
window.triggerFileInput = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.click();
    }
};

// Function to download file
window.downloadFile = function (fileName, contentType, content) {
    const byteArray = new Uint8Array(content);
    const blob = new Blob([byteArray], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}; 