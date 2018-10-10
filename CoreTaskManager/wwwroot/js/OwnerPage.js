window.onload = () => {
    let utt = new UnappliedTasksTable();
    let opTask = new OperateTaskForm();
}

class UnappliedTasksTable {
    constructor() {
        this.unauthenticatedTaskId;
        this.requestingUser;

        this.requestTable();
        $('#unappliedTaskTable').on('click', 'table>tbody', e => {
            this.unauthenticatedTaskId = e.target.parentNode.id;
            this.requestingUser = e.target.parentNode.firstChild.firstChild.data;
            $('#aSingleWord').text($('#words' + this.unauthenticatedTaskId).val());
            $('#approval').modal('show');
        });
        $('#approvalRequestButton').on('click', e => {
            this.approval(this.unauthenticatedTaskId, this.requestingUser);
        });
    }

    requestTable() {
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
            "<table id='table' class='table-hover'>" +
            "<thead>" +
            "<tr>" +
            "<th>ユーザ</th>" +
            "<th>題名</th>" +
            "<th>タスク名</th>" +
            "<th>申請日時</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>";
        for (let i = 0; i < tasksObject.length; i++) {
            let dt = new Date(tasksObject[i].achievedDateTime);
            console.log(dt);
            tag += "" +
                "<tr id=" + tasksObject[i].achivedTaskId + ">" +
                "<td>" + tasksObject[i].userName + "</td>" +
                "<td>" + tasksObject[i].progressName + "</td>" +
                "<td>" + tasksObject[i].taskName + "</td>" +
                "<td>" + (dt.getMonth() + 1) + "月" + dt.getDate() + "日" + dt.getHours() + "時" + dt.getMinutes() + "分" + "</td>" +
                "<input id='words" + tasksObject[i].achivedTaskId + "' type='hidden' value=" + tasksObject[i].description + "/>" +
                "</tr>"
                ;
        }
        tag += "" +
            "</tbody>" +
            "</table>";

        let resultTag = document.getElementById("unappliedTaskTable");
        resultTag.innerHTML = tag;
    }
    approval(id, user) {
        let sendData = {
            achievedTaskId: id,
            requestUser: user
        };
        $.ajax({
            type: "POST",
            data: JSON.stringify(sendData),
            url: "/OwnerPage?handler=Approval",
            beforeSend: xhr => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: response => {
                if (response === "serverError") {
                    return alert("サーバでエラーが発生しました");
                }
                if (response === "failed") {
                    return;
                }
                this.createTable(JSON.parse(response));
                console.log("success");
                $('#approval').modal('hide');
            },
            failure: response => {
                alert("通信に失敗しました");
            }
        });
    }
}
class OperateTaskForm {
    constructor() {
        // 初めはフォーム一つだけ
        this.countTask = 1;

        $('#addTask').on('click', e => {
            this.addTaskForm();
        });
        $('#removeTask').on('click', e => {
            this.removeTaskForm();
        });
        $('#modalClose').on('click', e => {
            this.destroyFirstForms();
            this.destroyAddedForms();
        });
        $('#transmitTaskData').on('click', e => {
            if (!this.isCaughtValidate()) {
                if (this.transmitTaskData() === "success") {
                    this.destroyFirstForms();
                    this.destroyAddedForms();
                    $('.modal-backdrop').remove(); 
                    $('#registerNew').modal('hide');
                    $('#modalButton').hide();
                }
            }
        });
    }

    addTaskForm() {
        if (this.countTask > 99) {
            this.countTask = 99;
        }
        this.countTask++;
        let tag;
        let divElement = document.createElement("div");
        divElement.setAttribute('class', 'form-group');
        divElement.setAttribute('id', this.countTask);

        tag = "" +
            "<div class='alert1' id='task" + this.countTask + "Alert'></div>" +
            "<label>・タスク" + this.countTask + "(15文字以内)</label>" +
            "<input type='text' id='task" + this.countTask + "' class='form-control' />";

        divElement.innerHTML = tag;
        let parentObject = document.getElementById("addedTask");
        parentObject.appendChild(divElement);
    }
    removeTaskForm() {
        if (this.countTask > 1) {
            let parentObject = document.getElementById("addedTask");
            let targetElement = document.getElementById(this.countTask);
            parentObject.removeChild(targetElement);
            this.countTask--;
        }
    }
    destroyFirstForms() {
        $('#task1').val('');
        $('#task1Alert').text('');
    }
    destroyAddedForms() {
        let addedTask = document.getElementById("addedTask");
        for (let i = 2; i <= this.countTask; i++) {
            let targetElement = document.getElementById(i);
            addedTask.removeChild(targetElement);
        }
        this.countTask = 1;
    }
    isCaughtValidate() {
        let catchValidateFlag = false;
        if ($('#task1').val() === "") {
            $('#task1Alert').text("*入力してください");
            catchValidateFlag = true;
        } else if ($('#task1').val().length > 15) {
            $('#task1Alert').text("*15文字以内で入力してください");
            catchValidateFlag = true;
        }
        let addedTask = document.getElementById("addedTask");
        for (let i = 2; i <= this.countTask; i++) {
            let inputStr = addedTask.childNodes[i - 2].childNodes[2];
            let alertElement = addedTask.childNodes[i - 2].childNodes[0];
            if (inputStr.value === "") {
                alertElement.textContent = "*入力してください";
                catchValidateFlag = true;
            } else if (inputStr.value.length > 15) {
                alertElement.textContent = "*15文字以内で入力してください";
                catchValidateFlag = true;
            }
        }
        return catchValidateFlag;
    }
    transmitTaskData() {
        // 入力された文字列（タスク名）取得
        let tasks = { task1: $('#task1').val() };
        let addedTask = document.getElementById("addedTask");
        for (var i = 2; i <= this.countTask; i++) {
            let formElement = addedTask.childNodes[i - 2].childNodes[2];
            tasks[formElement.id] = formElement.value;
        }

        let transmitSuccessFlag = true;
        $.ajax({
            type: "POST",
            data: JSON.stringify(tasks),
            url: "/OwnerPage?handler=SetTasks",
            beforeSend: xhr => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: response => {
                if (response === "serverError") {
                    alert("サーバでエラーが発生しました");
                    transmitSuccessFlag = false;
                }
                if (response === "wrongString") {
                    alert("不正な値が入力されました");
                    transmitSuccessFlag = false;
                }
            },
            failure: response => {
                alert("通信に失敗しました");
                transmitSuccessFlag = false;
            }
        });
        if (transmitSuccessFlag) {
            return "success";
        } else {
            return "failed";
        }
    }
}
