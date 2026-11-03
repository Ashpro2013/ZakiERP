let channel = new BroadcastChannel("CustomerDisplay");
let customerDisplayWindow = null;

window.customerDisplay = {

    openWindow: function () {
        if (customerDisplayWindow && !customerDisplayWindow.closed) {
            customerDisplayWindow.focus();
        } else {
            customerDisplayWindow = window.open(
                "/customerdisplay",
                "CustomerDisplayWindow",
                "width=900,height=700"
            );
            if (customerDisplayWindow) {
                customerDisplayWindow.focus();
            }
        }
    },

    sendData: function (data) {
        console.log("POS Sending:", data);
        channel.postMessage(data);
    },

    register: function (dotNetObject) {

        console.log("Customer Display Registered");

        channel.onmessage = function (e) {

            console.log("Customer Display Received:", e.data);

            dotNetObject.invokeMethodAsync(
                "ReceiveData",
                JSON.stringify(e.data)
            );

        };
    }
};
