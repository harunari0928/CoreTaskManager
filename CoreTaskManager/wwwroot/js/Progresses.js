
window.onload = () => {
    let page = new Paging();

    // ツールチップ
    $('[data-toggle="tooltip"]').tooltip();
};


class Paging {
    constructor() {
        $('.card').on('click', e => {
            this.transToTaskManager(e);
        });
    }

    transToTaskManager(e) {
        let progressId = e.currentTarget.id;
        window.location.href = "TaskManager/Index?progressIdString=" + progressId;
    }
}