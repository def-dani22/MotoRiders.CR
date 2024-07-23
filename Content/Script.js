// Función para desmarcar todos los checkboxes
function uncheckAllCheckboxes() {
    var checkboxes = document.querySelectorAll('.form-check-input');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
}


function agregarAlCarrito() {
    var checkboxes = document.querySelectorAll('.form-check-input:checked');
    var productosSeleccionados = [];

    checkboxes.forEach(function (checkbox) {
        productosSeleccionados.push(checkbox.value); // Asumiendo que el value del checkbox es el ID del producto
    });

    // Enviar los IDs de los productos seleccionados al controlador
    $.ajax({
        type: 'POST',
        url: '@Url.Action("AgregarAlCarrito", "Producto")', // Ajusta según tu controlador y acción
        data: { productos: productosSeleccionados },
        success: function (response) {
            // Redirigir a la vista del carrito después de agregar los productos
            window.location.href = '@Url.Action("Carrito", "Producto")';
        },
        error: function (error) {
            console.error('Error al agregar productos al carrito', error);
        }
    });
}


document.getElementById('btnCerrarSesion').addEventListener('click', function (event) {
    event.preventDefault(); // Prevenir el comportamiento predeterminado del enlace

    fetch(this.getAttribute('href'), {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({}), // Puedes enviar datos en el cuerpo si es necesario
    })
        .then(response => {
            if (response.ok) {
                window.location.href = '@Url.Action("InicioSesion", "Cuenta")'; // Redirigir al inicio de sesión
            } else {
                // Manejar errores si es necesario
                console.error('Error al cerrar sesión');
            }
        })
        .catch(error => {
            console.error('Error en la solicitud:', error);
        });
});




//CONTRASEÑA PARAMETROS
$(document).ready(function () {
    $('#newPassword').on('input', function () {
        var password = $(this).val();
        var length = password.length >= 14 && password.length <= 20;
        var lowercase = /[a-z]/.test(password);
        var uppercase = /[A-Z]/.test(password);
        var digit = /[0-9]/.test(password);
        var special = /[!@#$%^&*()_+<>?[\]{}|]/.test(password);

        $('#length').toggleClass('valid', length).toggleClass('invalid', !length);
        $('#lowercase').toggleClass('valid', lowercase).toggleClass('invalid', !lowercase);
        $('#uppercase').toggleClass('valid', uppercase).toggleClass('invalid', !uppercase);
        $('#digit').toggleClass('valid', digit).toggleClass('invalid', !digit);
        $('#special').toggleClass('valid', special).toggleClass('invalid', !special);
    });

    $('form').on('submit', function (e) {
        var newPassword = $('input[name="newPassword"]').val();
        var confirmNewPassword = $('input[name="confirmNewPassword"]').val();
        var errorMessage = '';

        if (newPassword !== confirmNewPassword) {
            errorMessage = 'Las contraseñas no coinciden.';
        } else if (newPassword.length < 14 || newPassword.length > 20) {
            errorMessage = 'La contraseña debe tener entre 14 y 20 caracteres.';
        } else if (!/[a-z]/.test(newPassword)) {
            errorMessage = 'La contraseña debe tener al menos una letra minúscula.';
        } else if (!/[A-Z]/.test(newPassword)) {
            errorMessage = 'La contraseña debe tener al menos una letra mayúscula.';
        } else if (!/[0-9]/.test(newPassword)) {
            errorMessage = 'La contraseña debe tener al menos un dígito.';
        } else if (!/[!@#$%^&*()_+<>?[\]{}|]/.test(newPassword)) {
            errorMessage = 'La contraseña debe tener al menos un carácter especial.';
        }

        if (errorMessage !== '') {
            e.preventDefault();
            alert(errorMessage);
        }
    });
});


