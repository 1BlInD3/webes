document.onkeydown = function (e) {

    if (e == null) { // ie
        keycode = event.keyCode;
    } else { // mozilla
        keycode = e.which;
    }

    var currentPage = parseInt(window.location.href.slice(-1));
    if (isNaN(currentPage)) currentPage = 0;

    if (keycode == 34) { //BACK (page down)
        currentPage--;
        if (currentPage < 1) currentPage = 4;
        window.location.replace("/Home/GroupView/" + currentPage);
    } else if (keycode == 33) { //NEXT (page up)
        currentPage++;
        if (currentPage > 4) currentPage = 1;
        window.location.href = "/Home/GroupView/" + currentPage;
    } else if (keycode == 49) { //1
        currentPage = 1;
        window.location.href = "/Home/GroupView/" + currentPage;
    } else if (keycode == 50) { //2
        currentPage = 2;
        window.location.href = "/Home/GroupView/" + currentPage;
    } else if (keycode == 51) { //3
        currentPage = 3;
        window.location.href = "/Home/GroupView/" + currentPage;
    } else if (keycode == 52) { //4
        currentPage = 4;
        window.location.href = "/Home/GroupView/" + currentPage;
    } else if (keycode == 53) { //5
        window.location.href = "/Home/DailyProd";
    } else if (keycode == 54) { //6
        window.location.href = "/Home/WeeklyProd";
    } else if (keycode == 413 | keycode == 36) { //STOP 413, HOME 36
        var fadingElements = document.getElementsByClassName("animated-layer");
        var k;
        for (k = 0; k < fadingElements.length; k++)
        {
            fadingElements[k].style.animationDelay = "0s";
        }
    } else if (keycode == 1004 | keycode == 35) { //T.OPT 1004, END 35
        var fadingElements = document.getElementsByClassName("animated-layer");
        var k;
        for (k = 0; k < fadingElements.length; k++) {
            fadingElements[k].style.animationDelay = "-30s";
        }
    }
}