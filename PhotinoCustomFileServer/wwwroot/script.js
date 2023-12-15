
setTimeout(function ()
{
    alert("Hellow From script!");
}, 50);


function callDotNet()
{
    window.external.sendMessage("Hi from browser.");
}

window.external.receiveMessage(message => receiveHandler(message));

function receiveHandler(message)
{
    alert(message);
}