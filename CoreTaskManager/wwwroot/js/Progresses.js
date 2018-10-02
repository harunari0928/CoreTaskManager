
window.onload = () => {
    let page = new Paging();
}


class Paging {
    constructor() {
        $('.card').on('click', e => {
            this.transToTaskManager(e);
        });
    }

    transToTaskManager(e) {
        let progressId = e.currentTarget.id;
        window.location.href = "TaskManager/Index?progressId=" + progressId;
    }
}