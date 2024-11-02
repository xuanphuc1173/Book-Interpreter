function showAlert() {
    let alertContainer = document.getElementById('alertContainer');

    // Tạo nội dung thông báo
    let alert = document.createElement('div');
    alert.className = 'custom-alert';
    alert.innerHTML = `
                <span class="btn-close" onclick="this.parentElement.remove()">x</span>
                <strong>Alert:</strong> This is a custom alert with a progress bar!
                <div class="progress-bar"></div>
            `;

    // Thêm thông báo vào container
    alertContainer.appendChild(alert);

    // Thêm logic cho thanh tiến trình
    let progressBar = alert.querySelector('.progress-bar');
    progressBar.style.animation = 'progress 5s linear forwards'; // Thêm animation cho thanh tiến trình


    // Tự động đóng thông báo sau 5 giây
    setTimeout(function () {
        alert.remove();
    }, 5000);

}
window.onload = function () {
    let alert = document.querySelector('.custom-alert');

    if (alert) {
        // Tự động đóng thông báo sau 5 giây
        setTimeout(function () {
            alert.remove();
        }, 5000);
    }
};
