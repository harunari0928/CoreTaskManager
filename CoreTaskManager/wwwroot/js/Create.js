let ctf;
window.onload = () => {
    ctf = new changeTaskForms(1);
};

class changeTaskForms {
    constructor(count) {
        this.countTask = count;
    }

    addTaskForm() {
        if (this.countTask > 20) {
            return;
        }
        this.countTask++;
        let tag;
        let divElement = document.createElement("div");
        divElement.setAttribute('class', 'form-group');
        divElement.setAttribute('id', this.countTask);

        tag = "" +
            "<div class='alert1' id='task" + this.countTask + "Alert'></div>" +
            "<label>タスク" + this.countTask + "(20文字以内)</label>" +
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
}