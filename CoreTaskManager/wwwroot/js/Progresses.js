window.onload = () => {
    $('.card').on('click' , e => {
        let progressId = e.currentTarget.id;
        window.location.href = "TaskManager/Index?progressId=" + progressId;
    });
}
