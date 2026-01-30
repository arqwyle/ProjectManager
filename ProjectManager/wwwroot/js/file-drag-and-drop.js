var style = document.createElement('style');
style.innerHTML = `
#drop-zone {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 500px;
    max-width: 100%;
    height: 200px;
    padding: 1em;
    border: 1px solid #cccccc;
    border-radius: 4px;
    color: slategray;
    cursor: pointer;
}

#file-input {
    display: none;
}

#preview {
    width: 500px;
    max-width: 100%;
    display: flex;
    flex-direction: column;
    gap: 0.5em;
    list-style: none;
    padding: 0;
}

#preview li {
    display: flex;
    align-items: center;
    gap: 0.5em;
    margin: 0;
    width: 100%;
    height: 100px;
}

#preview img {
    width: 100px;
    height: 100px;
    object-fit: cover;
}
`;
document.head.appendChild(style);

const dropZone = document.getElementById("drop-zone");
const preview = document.getElementById("preview");
const fileInput = document.getElementById("file-input");
const clearBtn = document.getElementById("clear-btn");

let uploadedFiles = [];

function dropHandler(ev) {
    ev.preventDefault();
    const files = [...ev.dataTransfer.items]
        .map((item) => item.getAsFile())
        .filter((file) => file);

    handleFiles(files);
}

fileInput.addEventListener("change", (e) => {
    handleFiles(e.target.files);
});

function handleFiles(files) {
    uploadedFiles = Array.from(files).filter(file => {
        if (file.size > 10 * 1024 * 1024) { // 10MB
            alert(`File ${file.name} is too large (max 10MB)`);
            return false;
        }
        return true;
    });

    updatePreview();
}

function updatePreview() {
    preview.innerHTML = '';
    uploadedFiles.forEach(file => {
        const li = document.createElement("li");

        if (file.type.startsWith("image/")) {
            const img = document.createElement("img");
            img.src = URL.createObjectURL(file);
            img.alt = file.name;
            img.style.maxWidth = "100px";
            img.style.maxHeight = "100px";
            li.appendChild(img);
        }

        li.appendChild(document.createTextNode(file.name));
        preview.appendChild(li);
    });
}

dropZone.addEventListener("dragover", (e) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "copy";
});

window.addEventListener("dragover", (e) => {
    e.preventDefault();
});

dropZone.addEventListener("drop", dropHandler);

clearBtn.addEventListener("click", () => {
    uploadedFiles.forEach(file => {
        if (file.type.startsWith("image/")) {
            URL.revokeObjectURL(file.previewUrl);
        }
    });
    uploadedFiles = [];
    preview.innerHTML = "";
});

window.uploadedFiles = uploadedFiles;