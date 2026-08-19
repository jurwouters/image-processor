window.imageProcessor = window.imageProcessor || {};

window.imageProcessor.getSelectedImageDimensions = async function (inputElementId) {
    const input = document.getElementById(inputElementId);

    if (!input || !input.files) {
        return [];
    }

    const files = Array.from(input.files);
    const dimensions = await Promise.all(files.map(getImageDimensions));
    return dimensions;
};

function getImageDimensions(file) {
    return new Promise((resolve) => {
        if (!file || (file.type && !file.type.startsWith("image/"))) {
            resolve({ width: null, height: null });
            return;
        }

        const imageUrl = URL.createObjectURL(file);
        const image = new Image();

        image.onload = () => {
            const width = Number.isFinite(image.naturalWidth) ? image.naturalWidth : null;
            const height = Number.isFinite(image.naturalHeight) ? image.naturalHeight : null;

            URL.revokeObjectURL(imageUrl);
            resolve({ width, height });
        };

        image.onerror = () => {
            URL.revokeObjectURL(imageUrl);
            resolve({ width: null, height: null });
        };

        image.src = imageUrl;
    });
}
