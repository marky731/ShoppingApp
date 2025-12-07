// ShoppingApp Custom JavaScript

$(function () {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow');
    }, 5000);

    // Confirm delete actions
    $('[data-confirm]').click(function (e) {
        if (!confirm($(this).data('confirm'))) {
            e.preventDefault();
        }
    });

    // Quantity input validation
    $('input[type="number"]').on('change', function () {
        var min = parseInt($(this).attr('min'));
        var max = parseInt($(this).attr('max'));
        var val = parseInt($(this).val());

        if (val < min) {
            $(this).val(min);
        } else if (val > max) {
            $(this).val(max);
        }
    });

    // AJAX cart count update
    function updateCartCount() {
        $.get('/Cart/Count', function (data) {
            if (data.count > 0) {
                $('.cart-count').text(data.count).show();
            } else {
                $('.cart-count').hide();
            }
        });
    }

    // Add to favorites via AJAX
    $('.btn-favorite').click(function (e) {
        e.preventDefault();
        var btn = $(this);
        var productId = btn.data('product-id');
        var action = btn.hasClass('favorited') ? 'Remove' : 'Add';

        $.post('/Favorites/' + action, { productId: productId }, function (result) {
            if (result.success) {
                btn.toggleClass('favorited btn-default btn-danger');
                btn.find('.btn-text').text(btn.hasClass('favorited') ? 'Remove from Favorites' : 'Add to Favorites');
            }
        });
    });

    // Product image gallery
    $('.product-gallery img').click(function () {
        var src = $(this).attr('src');
        $('#main-product-image').attr('src', src);
        $('.product-gallery img').removeClass('active');
        $(this).addClass('active');
    });

    // Form validation styling
    $.validator.setDefaults({
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });

    // Search autocomplete (placeholder for future implementation)
    // var searchInput = $('input[name="SearchTerm"]');
    // if (searchInput.length) {
    //     searchInput.autocomplete({
    //         source: '/Products/Search',
    //         minLength: 2
    //     });
    // }

    // Print order
    $('.btn-print-order').click(function () {
        window.print();
    });

    // Smooth scroll to top
    $(window).scroll(function () {
        if ($(this).scrollTop() > 100) {
            $('.scroll-to-top').fadeIn();
        } else {
            $('.scroll-to-top').fadeOut();
        }
    });

    $('.scroll-to-top').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 'slow');
        return false;
    });
});
