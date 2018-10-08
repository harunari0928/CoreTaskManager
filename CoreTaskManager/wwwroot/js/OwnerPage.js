window.onload = () => {
    let utt = new UnappliedTasksTable();
}

class UnappliedTasksTable {
    constructor() {
        this.requestTableObject();
    }

    requestTableObject() {
        $.ajax({
            type: "POST",
            url: "/OwnerPage?handler=SendUnappliedTable",
            beforeSend: xhr => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: response => {
                console.log("aaa");
                if (response === "serverError") {
                    return alert("サーバでエラーが発生しました");
                }
                if (response === "failed") {
                    return;
                }
                this.createTable(JSON.parse(response));
                console.log("success");
            },
            failure: response => {
                alert("通信に失敗しました");
            }
        });
    }
    createTable(tasksObject) {
        
        let tag;
        tag = "" +
            "<table id='table' class='table table-hover table-striped table-responsive'>" +
            "<thead class='thead-dark'>" +
            "<tr>" +
            "<th scope='col'>ユーザ</th>" +
            "<th scope='col'>題名</th>" +
            "<th scope='col'>タスク名</th>" +
            "<th scope='col'>申請日時</th>" +
            "<th scope='col'>一言</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>";
        for (let i = 0; i < tasksObject.length; i++) {
            tag += "" +
                "<tr id=" + tasksObject[i].achivedTaskId + ">" +
                "<td>" + tasksObject[i].userName + "</td>" +
                "<td>" + tasksObject[i].progressName + "</td>" +
                "<td>" + tasksObject[i].taskName + "</td>" +
                "<td>" + tasksObject[i].achievedDateTime + "</td>" +
                "<td>" + tasksObject[i].description + "</td>" +
                "</tr>";
        }
        tag += "" +
            "</tbody>" +
            "</table>";

        let resultTag = document.getElementById("unappliedTaskTable");
        resultTag.innerHTML = tag;
    }
}