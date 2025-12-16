// MINIMALSHOP - Modern JavaScript

$(function () {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Discount code apply button
    $(document).on('click', '#applyDiscountBtn', function () {
        var code = $('#discountCodeInput').val();
        var subtotal = parseFloat($('#checkoutSubtotal').data('subtotal')) || 0;
        var messageDiv = $('#discountMessage');

        if (!code || code.trim() === '') {
            messageDiv.html('<span class="text-danger">Please enter a discount code.</span>');
            return;
        }

        // Show loading state
        messageDiv.html('<span class="text-info">Applying...</span>');

        $.post('/Checkout/ApplyDiscount', { discountCode: code, subtotal: subtotal })
            .done(function (result) {
                if (result.success) {
                    messageDiv.html('<span class="text-success">' + result.message + '</span>');
                    $('#discountAmount').text('-$' + result.discountAmount.toFixed(2));
                    $('#total').text('$' + result.newTotal.toFixed(2));
                } else {
                    messageDiv.html('<span class="text-danger">' + result.message + '</span>');
                }
            })
            .fail(function (xhr, status, error) {
                messageDiv.html('<span class="text-danger">Error applying discount. Please try again.</span>');
                console.log('AJAX Error:', error);
            });
    });

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow');
    }, 5000);

    // Make elements with .clickable class navigate to their data-href
    $(document).on('click', '.clickable[data-href]', function (e) {
        // Don't navigate if clicking on a button, link, input, or interactive element
        if ($(e.target).closest('a, button, input, .btn, .btn-add-favorite, .btn-add-cart, .product-card-favorite').length) {
            return;
        }
        window.location.href = $(this).data('href');
    });

    // Make product cards clickable
    $(document).on('click', '.product-card[data-href]', function (e) {
        if ($(e.target).closest('button, .product-card-favorite, .btn-add-cart').length) {
            return;
        }
        window.location.href = $(this).data('href');
    });

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

    // AJAX favorites count update
    function updateFavoritesCount() {
        $.get('/Favorites/Count', function (data) {
            if (data.count > 0) {
                $('.favorites-count').text(data.count).show();
            } else {
                $('.favorites-count').hide();
            }
        });
    }

    // Load cart and favorites counts on page load
    updateCartCount();
    updateFavoritesCount();

    // Add to favorites via AJAX (old style for backward compatibility)
    $('.btn-favorite').click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var btn = $(this);
        var productId = btn.data('product-id');
        var action = btn.hasClass('favorited') ? 'Remove' : 'Add';

        $.post('/Favorites/' + action, { productId: productId }, function (result) {
            if (result.success) {
                btn.toggleClass('favorited btn-default btn-danger');
                btn.find('.btn-text').text(btn.hasClass('favorited') ? 'Remove from Favorites' : 'Add to Favorites');
                updateFavoritesCount();
            }
        });
    });

    // Modern product card - Add to favorites
    $(document).on('click', '.btn-add-favorite, .product-card-favorite', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var btn = $(this);
        var productId = btn.data('product-id');
        var isFavorited = btn.hasClass('favorited');
        var action = isFavorited ? 'Remove' : 'Add';

        $.post('/Favorites/' + action, { productId: productId }, function (result) {
            if (result.success) {
                btn.toggleClass('favorited');
                var icon = btn.find('.heart-icon, .glyphicon');
                if (btn.hasClass('favorited')) {
                    icon.removeClass('glyphicon-heart-empty').addClass('glyphicon-heart');
                } else {
                    icon.removeClass('glyphicon-heart').addClass('glyphicon-heart-empty');
                }
                updateFavoritesCount();
            }
        });
    });

    // Modern product card - Add to cart
    $(document).on('click', '.btn-add-cart', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var btn = $(this);
        var productId = btn.data('product-id');
        var originalText = btn.text();

        btn.prop('disabled', true).text('Adding...');

        $.post('/Cart/AddJson', { productId: productId, quantity: 1 }, function (result) {
            if (result.success) {
                btn.text('Added!');
                updateCartCount();
                setTimeout(function () {
                    btn.prop('disabled', false).text(originalText);
                }, 1500);
            } else {
                btn.prop('disabled', false).text(originalText);
                alert(result.message || 'Error adding to cart');
            }
        }).fail(function () {
            btn.prop('disabled', false).text(originalText);
            alert('Error adding to cart. Please try again.');
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
    if ($.validator) {
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
    }

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

    // Category dropdown hover (for desktop)
    if ($(window).width() > 768) {
        $('.category-nav .dropdown').hover(
            function () {
                $(this).find('.dropdown-menu').stop(true, true).fadeIn(200);
            },
            function () {
                $(this).find('.dropdown-menu').stop(true, true).fadeOut(200);
            }
        );
    }

    // Mobile menu toggle
    $('.navbar-toggle').click(function () {
        $(this).toggleClass('active');
    });
});
