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
