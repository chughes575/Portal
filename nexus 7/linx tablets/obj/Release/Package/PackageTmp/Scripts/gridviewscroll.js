function gridviewScroll() {
    $('#<%=GridView1.ClientID%>').gridviewScroll({
        width: 660,
        height: 200,
        arrowsize: 30,
        varrowtopimg: "Images/arrowvt.png",
        varrowbottomimg: "Images/arrowvb.png",
        harrowleftimg: "Images/arrowhl.png",
        harrowrightimg: "Images/arrowhr.png"
    });
}