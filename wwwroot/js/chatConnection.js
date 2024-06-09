// Определение глобальной переменной connection
window.connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.onclose(error => {
    console.log("Connection closed: ", error);
    connection.start().catch(err => console.error("Reconnection Error: ", err.toString()));
});

connection.start()
    .then(() => {
        console.log("SignalR Connected.");
        connection.invoke("JoinChat", userEmail, receiverId);

        connection.invoke("StartCall", receiverId)
            .then(() => console.log("Call started successfully"))
            .catch(err => console.error("Error starting call: ", err.toString()));
    })
    .catch(err => console.error("SignalR Connection Error: ", err.toString()));


document.getElementById("startCallButton").addEventListener("click", () => {
    const receiverId = document.getElementById("receiverId").value;
    connection.invoke("StartCall", receiverId)
        .then(() => console.log("Call started successfully"))
        .catch(err => console.error("Error starting call: ", err.toString()));
});

connection.on("ReceiveCall", (callerId) => {
    console.log("Incoming call from: ", callerId);
    alert(`Входящий звонок от пользователя с ID: ${callerId}`);

    // Здесь можно добавить логику отображения окна с видео или других действий
    // Например, перенаправление на страницу видеозвонка или показ модального окна
});