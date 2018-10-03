window.onload = () => {
    let opTask = new OperateTaskForm();
};

class OperateTaskForm {
    constructor() {
        // 初めはフォーム一つだけ
        this.countTask = 1;
        this.tasks;
        $('#addTask').on('click', e => {
            this.countTask = this.addTaskForm(this.countTask);
        });
        $('#removeTask').on('click', e => {
            this.countTask = this.removeTaskForm(this.countTask);
        });
        $('.modalClose').on('click', e => {
            $('#task1Alert').text('');
            this.countTask = this.destroyAddedForms(this.countTask);
        });
        $('#transmitTaskData').on('click', e => {
            
            if (!this.caughtValidate(this.countTask)) {
                // TODO: TransmitAjax
                this.transmitTaskData();
            }
            
        });
    }
    
    addTaskForm(countTask) {
        if (countTask > 99) {
            return 99;
        }
        countTask++;
        let tag;
        let divElement = document.createElement("div");
        divElement.setAttribute('class', 'form-group');
        divElement.setAttribute('id', countTask);

        tag = "" +
            "<div class='alert1' id='task" + countTask + "Alert'></div>" +
            "<label>・タスク" + countTask + "(15文字以内)</label>" +
            "<input type='text' id='task" + countTask + "' class='form-control' />";

        divElement.innerHTML = tag;
        let parentObject = document.getElementById("addedTask");
        parentObject.appendChild(divElement);
        return countTask;
    }
    removeTaskForm(countTask) {
        if (countTask > 1) {
            let parentObject = document.getElementById("addedTask");
            let targetElement = document.getElementById(countTask);
            parentObject.removeChild(targetElement);
            countTask--;
        }
        return countTask;
    }
    destroyAddedForms(countTask) {
        let addedTask = document.getElementById("addedTask");
        for (let i = 2; i <= countTask; i++) {
            let targetElement = document.getElementById(i);
            addedTask.removeChild(targetElement);
        }
        return 1;
    }
    caughtValidate(countTask) {
        let catchValidateFlag = false;
        if ($('#task1').val() === "") {
            $('#task1Alert').text("*入力してください");
            catchValidateFlag = true;
        } else if ($('#task1').val().length > 10) {
            $('#task1Alert').text("*10文字以内で入力してください");
            catchValidateFlag = true;
        }
        let addedTask = document.getElementById("addedTask");
        for (let i = 2; i <= countTask; i++) {
            let inputStr = addedTask.childNodes[i - 2].childNodes[2];
            let alertElement = addedTask.childNodes[i - 2].childNodes[0];
            if (inputStr.value === "") {
                alertElement.textContent = "*入力してください";
                catchValidateFlag = true;
            } else if (inputStr.value.length > 10) {
                alertElement.textContent = "*10文字以内で入力してください";
                catchValidateFlag = true;
            }
        }
        return catchValidateFlag;    
    }
    transmitTaskData() {
        // TODO: 情報取得
        
        // TODO: ajax通信
        $.ajax({
            type: "POST",
            data: JSON.stringify({
                "ProgressId": "item1",
                "TaskName": "item2"
            }),
            url: "/TaskManager?handler=Send",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert("Success");
            },
            failure: function (response) {
                alert(response);
            }
        });

        // フォームの中身削除
        this.countTask = this.removeAllForm(this.countTask);
        $('#registerNew').modal('hide');
    }
}
