/*
=======================================================
* Script Name: BingoScript - v1.0
* Author: Crivelli José Marcos
* License: https://github.com/JMC2022/JMC2022.github.io/blob/main/LICENSE
======================================================== */
/* Sidebar Functions */
function openNav() {
    document.getElementById("sidebarMenu").style.width = "220px";
    document.getElementById("main").style.marginLeft = "-230px";
    let saloon = document.querySelector('#saloon');
    let menu = document.querySelector('#myMenu');
    let container = document.querySelector('#containerMenu');
    if(menu != null && container !=null && saloon != null)
    {
        menu.style.height="0";
        container.style.top="-136%";
        saloon.style.opacity = "30%";
    }
    let start = document.querySelector('#_start');
    if(start != null)
    {
        start.removeAttribute("onclick");
    }
    let launchBall = document.querySelector('#launch_ball');
    if(launchBall != null)
    {
        launchBall.removeAttribute("onclick");
    }
}
function closeNav() {
    document.getElementById("sidebarMenu").style.width = "0";
    document.getElementById("main").style.marginLeft = "0";
    let start = document.querySelector('#_start');
    let saloon = document.querySelector('#saloon');
    let launchBall = document.querySelector('#launch_ball');
    if(saloon != null){
        saloon.style.opacity = "100%";
    }
    if(start != null)
    {
        start.setAttribute("onclick","start('/Home/Start')");
    }
    if(launchBall != null){
        document.querySelector('#launch_ball').setAttribute("onclick","launchBall('/Home/LaunchBall')");
    }
}

/* My Menu Functions */
function openMenu(){
    document.getElementById("myMenu").style.height = "350px";
    document.getElementById("containerMenu").style.top = "0%";
    document.getElementById("sidebarMenu").style.width = "0";
    document.getElementById("main").style.marginLeft = "0";
    document.getElementById("saloon").style.opacity = "30%";
    let start = document.querySelector('#_start');
    if(start != null)
    {
        start.removeAttribute("onclick");
    }
    let launchBall = document.querySelector('#launch_ball');
    if(launchBall != null)
    {
        launchBall.removeAttribute("onclick");
    }
}
function closeMenu(){
    document.getElementById("myMenu").style.height = "0";
    document.getElementById("containerMenu").style.top = "-136%";
    document.getElementById("saloon").style.opacity = "100%";
    let start = document.querySelector('#_start');
    let launchBall = document.querySelector('#launch_ball');
    if(start != null)
    {
        start.setAttribute("onclick","start('/Home/Start')");;
    }
    if(launchBall != null){
        document.querySelector('#launch_ball').setAttribute("onclick","launchBall('/Home/LaunchBall')");
    }
}

/* SALOON FUNCTIONS */

/*<Get tickets>*/
let canT;
function choose(cant, url){
    document.getElementById("launchB").innerHTML = `<a id="_start" class="btn-md" href="javascript:void(0)"></a>`;
    document.getElementById("_start").style.backgroundImage=`url("/img/Start-btn.png")`;
    document.querySelector('#grid_ticket').innerHTML = ``;
    const param = {cantT:cant};
    let loop = true;
    $.get(url, param).done(function (Tkts){
        while(loop){
              $("#grid_ticket").append(Tkts);
              cantT = cant;
              loop = false;
              }
    }).always(function (){
            document.querySelector('#choose1').setAttribute("onclick",`choose(1,'${url}')`);
            document.querySelector('#choose2').setAttribute("onclick",`choose(2,'${url}')`);
            document.querySelector('#choose3').setAttribute("onclick",`choose(3,'${url}')`);
            document.querySelector('#choose4').setAttribute("onclick",`choose(4,'${url}')`);
             });
if ((cant > 0 || cant <5) && loop === true){
                $("#choose1").prop('onclick', false);
                $("#choose2").prop('onclick', false);
                $("#choose3").prop('onclick', false);
                $("#choose4").prop('onclick', false);
              }
}

/*<Start>*/
function start(url){
    document.querySelector('#_container').innerHTML = `<p>¡Good Luck!</p>`;
    document.querySelector('#_bingoBall').innerHTML = `<img src="/img/BingoBallRolling.gif"/>`;
    playing();
    $.post(url);
}

/*<Sort Html for playing>*/
function playing(){
    document.getElementById("titleChoose").innerHTML = `<a id="titleChoose">Now Paying With</a>`;
    document.getElementById("_choose").innerHTML = `<p><img src="/img/choose-${cantT} .png"/></p>`;
    document.getElementById("_start").remove();
    document.getElementById("launchB").innerHTML = `<a id="launch_ball" class="btn-md" ></a>
        <a id="ball_number" class="btn-md"></a>`;
    document.querySelector('#launch_ball').setAttribute("onclick","launchBall('/Home/LaunchBall')");
    document.getElementById("launch_ball").style.backgroundImage=`url("/img/Launch-btn.png")`;
    document.getElementById("ball_number").style.backgroundImage=`url("/img/Ball-Num.png")`;
}
/*Launch Ball and Print Acert -> Call PrintAcert*/
function launchBall(url){
    let loop = true;
    let press = 1;
    $.get(url).done(function(data){
    while(loop){
            document.querySelector('#ball_number').innerHTML = ``;
            $('#ball_number').append(data);
            loop = false;
            press =0;
            }
        }).always(function (){
            acert('/Home/CompareAcert');
           });
    if (press = 1) {
        document.querySelector('#launch_ball').removeAttribute("onclick");
        document.querySelector('#ball_number').innerHTML = `<img src="/img/Launching.gif"/>`;
    }
}
/*Match acert and Print color to the View -> Call checkWin */ 
function acert(url)
{
    $.get(url).done(function(data){
        if(data != undefined){
           for (let step = 0; step < data.length; step++) {
                const arr = data[step];
                const TicketNo= arr.ticketNo;
                const NumId= arr.numId;
                const Acert= arr.acert;
                const Num= arr.num;
                const indx = [2,11,20,3,12,21,4,13,22,5,14,23,6,15,24,7,16,25,8,17,26,9,18,27,10,19,28,];
                const i= indx[`${NumId-1}`];
                const printAcert=document.getElementById(`_grid_nums_${TicketNo}`).getElementsByTagName('div')[i].style.border=`2.2px solid red`;
           }        
        }
    }).always(function (){
            checkWin('/Home/CheckWin');
           });
}

/*Check Line or Bingo*/
function checkWin(url){
    $.get(url).done(function(data){
        if(data!=""){
            $('#containerWin').append(data);
        }
        else{
            document.querySelector('#launch_ball').setAttribute("onclick", "launchBall('/Home/LaunchBall')");
        }
    });
}
/*Continue Playing*/
function goOn(){
    document.querySelector('#launch_ball').setAttribute("onclick",`launchBall('/Home/LaunchBall')`);
    document.querySelector('#containerWin').innerHTML = ``;
}