document.addEventListener('DOMContentLoaded', () => {
    // --- Khai báo các biến cần thiết ---
    const slides = document.querySelectorAll('.slide');
    const dotsContainer = document.querySelector('.slider-dots');
    const prevBtn = document.querySelector('.prev-btn');
    const nextBtn = document.querySelector('.next-btn');

    let currentSlide = 0;
    let slideInterval; // Biến để chứa bộ đếm thời gian tự động chuyển slide

    const slideCount = slides.length;
    if (slideCount === 0) return; // Nếu không có slide nào thì dừng

    // --- Tạo các dấu chấm điều khiển ---
    for (let i = 0; i < slideCount; i++) {
        const dot = document.createElement('span');
        dot.classList.add('dot');
        dot.dataset.slide = i;
        dotsContainer.appendChild(dot);
    }
    const dots = document.querySelectorAll('.dot');
    dots[0].classList.add('active'); // Kích hoạt dấu chấm đầu tiên

    // --- Hàm chính để hiển thị slide ---
    function showSlide(slideIndex) {
        // Xử lý vòng lặp: nếu đến slide cuối thì quay lại slide đầu
        if (slideIndex >= slideCount) {
            slideIndex = 0;
        } else if (slideIndex < 0) {
            slideIndex = slideCount - 1;
        }

        // Ẩn slide hiện tại và bỏ active ở dot
        slides[currentSlide].classList.remove('active');
        dots[currentSlide].classList.remove('active');

        // Hiện slide mới và active dot mới
        slides[slideIndex].classList.add('active');
        dots[slideIndex].classList.add('active');

        // Cập nhật lại vị trí slide hiện tại
        currentSlide = slideIndex;
    }

    // --- CÁC HÀM MỚI ĐỂ TỰ ĐỘNG CHUYỂN SLIDE ---

    // Hàm để dừng slideshow (khi người dùng tương tác)
    function stopSlideShow() {
        clearInterval(slideInterval);
    }

    // Hàm để bắt đầu hoặc reset lại slideshow
    function startSlideShow() {
        stopSlideShow(); // Dừng slideshow cũ trước khi bắt đầu cái mới

        // Hàm setInterval sẽ lặp lại hành động bên trong nó
        // sau mỗi 10000 mili giây (tức là 10 giây)
        slideInterval = setInterval(() => {
            showSlide(currentSlide + 1);
        }, 10000);
    }

    // --- Gán sự kiện cho các nút ---
    nextBtn.addEventListener('click', () => {
        showSlide(currentSlide + 1);
        startSlideShow(); // Reset lại bộ đếm 10 giây khi người dùng tự bấm nút
    });
    hi

    prevBtn.addEventListener('click', () => {
        showSlide(currentSlide - 1);
        startSlideShow(); // Reset lại bộ đếm 10 giây
    });

    // Gán sự kiện cho các dấu chấm
    dots.forEach(dot => {
        dot.addEventListener('click', () => {
            const slideIndex = parseInt(dot.dataset.slide);
            showSlide(slideIndex);
            startSlideShow(); // Reset lại bộ đếm 10 giây
        });
    });

    // --- Tự động bắt đầu slideshow khi tải trang ---
    startSlideShow();
});