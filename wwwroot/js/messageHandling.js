
const userEmailsParsed = userEmails; 

const processedMessageIds = new Set();


function createMessageHtml(senderId, message, timestamp, messageId) {
    const isSender = userEmailsParsed[senderId] === userEmail;
    const senderEmail = userEmailsParsed[senderId] || "Неизвестный";
    const messageClass = isSender ? 'sent' : 'received';

    return `
        <li class="mb-3" id="message-${messageId}">
            <div class="message ${messageClass}">
                <div class="message-content">
                    <strong>${senderEmail}</strong>
                    <span class="text-muted small">${timestamp}</span>
                    <p class="message-text">${message}</p>
                </div>
            </div>
        </li>
    `;
}


connection.on("ReceiveMessage", (senderId, message, messageId) => {

    if (processedMessageIds.has(messageId)) {
        return; 
    }

    const senderEmail = userEmails[senderId];
    if (!senderEmail || senderEmail === "Неизвестный") {
        console.log(`Сообщение от ${senderId} игнорировано, так как отправитель не идентифицирован.`);
        return;
    }

    processedMessageIds.add(messageId);

    const messageList = document.getElementById("messageList");
    const newMessageHtml = createMessageHtml(senderId, message, new Date().toLocaleTimeString(), messageId);
    messageList.insertAdjacentHTML('beforeend', newMessageHtml);
    messageList.scrollTop = messageList.scrollHeight; 
});

connection.on("MessageSent", (senderId, message, messageId) => {
    if (processedMessageIds.has(messageId)) {
        return; 
    }

    const senderEmail = userEmails[senderId];
    if (!senderEmail || senderEmail === "Неизвестный") {
        console.log(`Сообщение от ${senderId} игнорировано, так как отправитель не идентифицирован.`);
        return;
    }
    processedMessageIds.add(messageId);

    const messageList = document.getElementById("messageList");
    const newMessageHtml = createMessageHtml(senderId, message, new Date().toLocaleTimeString(), messageId);
    messageList.insertAdjacentHTML('beforeend', newMessageHtml);
    messageList.scrollTop = messageList.scrollHeight; 
});



connection.on("DeleteMessage", (messageId) => {
    const messageElement = document.getElementById(`message-${messageId}`);
    if (messageElement) {
        messageElement.remove();
    }
});

connection.on("EditMessage", (messageId, newText) => {
    const messageElement = document.getElementById(`message-${messageId}`);
    if (messageElement) {
        const textElement = messageElement.querySelector(".message-text");
        if (textElement) {
            textElement.textContent = newText;
        }
    }
});