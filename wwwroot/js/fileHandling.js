
function createFileMessageHtml(senderId, fileName, fileUrl, timestamp) {
    const isSender = userEmailsParsed[senderId] === userEmail;
    const senderEmail = userEmailsParsed[senderId] || "Неизвестный";
    const messageClass = isSender ? 'sent' : 'received';

    const fileExtension = fileUrl.split('.').pop().toLowerCase();
    const imageExtensions = ["jpg", "jpeg", "png", "gif", "bmp"];
    let fileHtml = '';

    if (imageExtensions.includes(fileExtension)) {
        fileHtml = `<img src="${fileUrl}" alt="${fileName}" class="img-fluid mt-2" style="max-width: 200px; max-height: 200px;" />`;
    } else {
        fileHtml = `<a href="${fileUrl}" target="_blank">${fileName}</a>`;
    }

    return `
        <li class="mb-3">
            <div class="message ${messageClass}">
                <div class="message-content">
                    <strong>${senderEmail}</strong>
                    <span class="text-muted small">${timestamp}</span>
                    <div class="message-file">${fileHtml}</div>
                </div>
            </div>
        </li>
    `;
}

connection.on("ReceiveFile", (senderId, fileName, fileUrl) => {
    const messageList = document.getElementById("messageList");
    const newFileMessageHtml = createFileMessageHtml(senderId, fileName, fileUrl, new Date().toLocaleTimeString());
    messageList.insertAdjacentHTML('beforeend', newFileMessageHtml);
    messageList.scrollTop = messageList.scrollHeight; 
});
