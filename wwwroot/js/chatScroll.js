document.addEventListener('DOMContentLoaded', () => {
    const chatContainer = document.getElementById('chatContainer');
    const messageList = document.getElementById('messageList');


    function scrollToBottom() {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }

    function isUserNearBottom() {
        return chatContainer.scrollHeight - chatContainer.scrollTop <= chatContainer.clientHeight + 50;
    }

    const scrollPosition = localStorage.getItem('chatScrollPosition');
    if (scrollPosition) {
        chatContainer.scrollTop = scrollPosition;
    } else {
        
        scrollToBottom();
    }

    
    window.addEventListener('beforeunload', () => {
        localStorage.setItem('chatScrollPosition', chatContainer.scrollTop);
    });



  
    const scrollToBottomAfterSend = localStorage.getItem('scrollToBottom');
    if (scrollToBottomAfterSend === 'true') {
        scrollToBottom();
        localStorage.removeItem('scrollToBottom');
    }


    if (typeof connection !== 'undefined') {
        connection.on("ReceiveMessage", (senderId, message) => {
        
            const newMessage = document.createElement("li");
            newMessage.classList.add("mb-3");
            newMessage.innerHTML = `
                <div class="message ${senderId === "@User.Identity.Name" ? "sent" : "received"}">
                    <div class="message-content">
                        <strong>${senderId}</strong>
                        <span class="text-muted small">${new Date().toLocaleTimeString()}</span>
                        <p>${message}</p>
                    </div>
                </div>
            `;
            messageList.appendChild(newMessage);

      
            if (isUserNearBottom()) {
                scrollToBottom();
            }
        });
    }
});
