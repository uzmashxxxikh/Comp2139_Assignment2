// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function() {
    // Handle form submissions
    $('form').on('submit', function(e) {
        // Check if the form has the data-ajax attribute
        if ($(this).data('ajax') !== true) {
            // For non-AJAX forms, just let them submit normally
            return true;
        }
        
        e.preventDefault();
        
        // Show loading spinner
        $('#loadingSpinner').show();
        
        // Get form data
        var formData = $(this).serialize();
        
        // Submit form via AJAX
        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            success: function(result) {
                // Hide loading spinner
                $('#loadingSpinner').hide();
                
                if (result.success) {
                    // Show success message
                    alert(result.message);
                    
                    // Redirect if needed
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else {
                        // Refresh the page
                        window.location.reload();
                    }
                } else {
                    // Show error message
                    alert(result.message || 'An error occurred.');
                }
            },
            error: function() {
                // Hide loading spinner
                $('#loadingSpinner').hide();
                
                // Show error message
                alert('An error occurred while processing your request.');
            }
        });
        
        return false;
    });
    
    // Handle product search form
    $('#searchForm').on('submit', function(e) {
        e.preventDefault();
        
        // Show loading spinner
        $('#searchSpinner').show();
        
        // Get form data
        var formData = $(this).serialize();
        
        // Submit form via AJAX
        $.ajax({
            url: '/Product/Search',
            type: 'GET',
            data: formData,
            success: function(result) {
                // Hide loading spinner
                $('#searchSpinner').hide();
                
                // Update product list
                $('#productList').html(result);
            },
            error: function() {
                // Hide loading spinner
                $('#searchSpinner').hide();
                
                // Show error message
                alert('An error occurred while searching for products.');
            }
        });
        
        return false;
    });
    
    // Handle input changes for product search
    $('#searchForm input, #searchForm select').on('change', function() {
        $('#searchForm').submit();
    });
});
