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


    document.getElementById('btnCerrarSesion').addEventListener('click', function(event) {
        event.preventDefault(); // Prevenir el comportamiento predeterminado del enlace

    fetch(this.getAttribute('href'), {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json',
            },
    body: JSON.stringify({ }), // Puedes enviar datos en el cuerpo si es necesario
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

