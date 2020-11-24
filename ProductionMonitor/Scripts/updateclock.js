var xmlHttp;
function getServerTime()
{
    try {
        //FF, Opera, Safari, Chrome
        xmlHttp = new XMLHttpRequest();
    }
    catch (err1) {
        //IE
        try {
            xmlHttp = new ActiveXObject('Msxml2.XMLHTTP');
        }
        catch (err2) {
            try {
                xmlHttp = new ActiveXObject('Microsoft.XMLHTTP');
            }
            catch (eerr3) {
                //AJAX not supported, use CPU time.
                alert("AJAX not supported");
            }
        }
    }
    xmlHttp.open('HEAD', window.location.href.toString(), false);
    xmlHttp.setRequestHeader("Content-Type", "text/html");
    xmlHttp.send('');
    return new Date(xmlHttp.getResponseHeader("Date"));
}

var now = getServerTime();
function updateClock() {
    now.setSeconds(now.getSeconds() + 1);

    var str = now.getFullYear() + '-';
    if (now.getMonth() + 1 < 10) str += '0';
    str += (now.getMonth() + 1) + '-';
    if (now.getDate() < 10) str += '0';
    str += now.getDate();
    document.getElementById('date').innerHTML = str;

    str = '';
    if (now.getHours() < 10) str += '0';
    str += now.getHours() + ':';
    if (now.getMinutes() < 10) str += '0';
    str += now.getMinutes() + ':';
    if (now.getSeconds() < 10) str += '0';
    str += now.getSeconds();
    document.getElementById('time').innerHTML = str;
    
    // call this function again in 1000ms
    setTimeout(updateClock, 1000);
}

