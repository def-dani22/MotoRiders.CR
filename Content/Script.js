// Variables para tiempos de animación
const time_to_show_login = 400;
const time_to_hidden_login = 200;
const time_to_show_sign_up = 100;
const time_to_hidden_sign_up = 400;
const time_to_hidden_all = 500;

// Funciones para cambiar entre vistas de login y sign up
function change_to_login() {
    document.querySelector('.cont_forms').className = "cont_forms cont_forms_active_login";
    document.querySelector('.cont_form_login').style.display = "block";
    document.querySelector('.cont_form_login').style.opacity = "0";

    setTimeout(function () {
        document.querySelector('.cont_form_login').style.opacity = "1";
    }, time_to_show_login);

    document.querySelector('.cont_form_sign_up').style.opacity = "0";
    setTimeout(function () {
        document.querySelector('.cont_form_sign_up').style.display = "none";
    }, time_to_hidden_login);
}

function change_to_sign_up() {
    document.querySelector('.cont_forms').className = "cont_forms cont_forms_active_sign_up";
    document.querySelector('.cont_form_sign_up').style.display = "block";
    document.querySelector('.cont_form_sign_up').style.opacity = "0";

    setTimeout(function () {
        document.querySelector('.cont_form_sign_up').style.opacity = "1";
    }, time_to_show_sign_up);

    document.querySelector('.cont_form_login').style.opacity = "0";
    setTimeout(function () {
        document.querySelector('.cont_form_login').style.display = "none";
    }, time_to_hidden_sign_up);
}

function hidden_login_and_sign_up() {
    document.querySelector('.cont_forms').className = "cont_forms";
    document.querySelector('.cont_form_sign_up').style.opacity = "0";
    document.querySelector('.cont_form_login').style.opacity = "0";

    setTimeout(function () {
        document.querySelector('.cont_form_sign_up').style.display = "none";
        document.querySelector('.cont_form_login').style.display = "none";
    }, time_to_hidden_all);
}

// Función para desmarcar todos los checkboxes
function uncheckAllCheckboxes() {
    var checkboxes = document.querySelectorAll('.form-check-input');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
}

// Array para almacenar los ítems seleccionados (puedes utilizar sessionStorage o localStorage para persistencia si es necesario)
let cartItems = [];

// Función para seleccionar un ítem y marcar el checkbox
function selectItem(id, nombre, precio) {
    // Aquí puedes realizar cualquier lógica adicional si es necesario
    // Por ejemplo, puedes verificar si el ítem ya está en el carrito antes de agregarlo

    // Ejemplo: Agregar el ítem al carrito (en este caso, solo el nombre y el precio)
    cartItems.push({ id: id, nombre: nombre, precio: precio });

    // Mostrar mensaje de éxito o actualizar UI (puedes adaptar esta parte según tu diseño)
    alert(`"${nombre}" agregado al carrito`);

    // Opcional: Puedes desmarcar automáticamente el checkbox si es necesario
    // var checkbox = document.querySelector(`input[value='${id}']`);
    // checkbox.checked = false;
}

// Función para manejar la lógica de agregar al carrito
function addToCart() {
    // Llamar a la función para desmarcar todos los checkboxes
    uncheckAllCheckboxes();

    // Aquí puedes agregar la lógica adicional para agregar elementos al carrito
    // Por ejemplo, podrías enviar cartItems al servidor para procesar la compra
    console.log('Elementos seleccionados:', cartItems);

    // Mostrar algún tipo de confirmación al usuario de que los productos fueron agregados al carrito
    var tooltip = document.querySelector('.buttonC');
    tooltip.setAttribute('data-tooltip', 'Agregado al carrito');
    setTimeout(function () {
        tooltip.setAttribute('data-tooltip', 'Agregado');
    }, 1500); // Cambia el tooltip de vuelta después de 1.5 segundos

    // Limpia el array de carrito para una nueva selección
    cartItems = [];
}

// Event listener para el botón "Agregar al Carrito"
document.querySelector('.buttonC').addEventListener('click', function () {
    addToCart();
});

// Event listener para el botón "Seleccionar"
document.querySelectorAll('.buttonS').forEach(button => {
    button.addEventListener('click', function () {
        var card = button.closest('.card');
        var id = card.querySelector('.form-check-input').value;
        var nombre = card.querySelector('.card-title').textContent;
        var precio = parseFloat(card.querySelector('.card-text strong').textContent.replace('Precio: ', '').replace('$', '').replace(',', ''));
        selectItem(id, nombre, precio);
    });
});
