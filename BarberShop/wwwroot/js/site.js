$(document).ready(function () {


    var loginPage = document.getElementById('loginPage');

    if (loginPage) {
        // Formulário de recuperação de senha
        const forgotPasswordForm = $("#forgotPasswordForm");
        const forgotPasswordEmailInput = $("#forgotPasswordEmail");
        const forgotPasswordErrorMessage = $("#forgotPasswordErrorMessage");

        forgotPasswordForm.on("submit", function (event) {
            event.preventDefault();
            const usuarioEmailRecuperarHtml = forgotPasswordEmailInput.val();  // Coleta o e-mail da entrada
            showToast("Formulário de recuperação de senha enviado com email: " + usuarioEmailRecuperarHtml, 'info');

            $.ajax({
                url: "/Login/SolicitarRecuperacaoSenha",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(usuarioEmailRecuperarHtml),  // Enviando apenas a string
                success: function (data) {
                    if (data.success) {
                        $('#forgotPasswordModal').modal('hide');
                        showToast("Instruções de recuperação de senha foram enviadas para o seu e-mail.", 'success');
                    } else {
                        showToast(data.message || "Erro ao solicitar recuperação de senha.", 'danger');
                    }
                },
                error: function (error) {
                    showToast("Erro no envio da recuperação de senha: " + error.message, 'danger');
                }
            });
        });
    }

    var redefinirSenhaPage = document.getElementById('redefinirSenhaPage');

    if (redefinirSenhaPage) {
        $('#novaSenhaRedefinir').on('input', function () {
            const password = $(this).val();
            const lengthRequirement = password.length >= 8;
            const uppercaseRequirement = /[A-Z]/.test(password);
            const lowercaseRequirement = /[a-z]/.test(password);
            const numberRequirement = /\d/.test(password);
            const specialRequirement = /[!@#$%^&*]/.test(password);

            $('#lengthRequirementRedefinir').toggleClass('text-success', lengthRequirement).toggleClass('text-danger', !lengthRequirement);
            $('#uppercaseRequirementRedefinir').toggleClass('text-success', uppercaseRequirement).toggleClass('text-danger', !uppercaseRequirement);
            $('#lowercaseRequirementRedefinir').toggleClass('text-success', lowercaseRequirement).toggleClass('text-danger', !lowercaseRequirement);
            $('#numberRequirementRedefinir').toggleClass('text-success', numberRequirement).toggleClass('text-danger', !numberRequirement);
            $('#specialRequirementRedefinir').toggleClass('text-success', specialRequirement).toggleClass('text-danger', !specialRequirement);
        });

        // Toggle de visibilidade das senhas
        $('#toggleNovaSenhaRedefinir').on('click', function () {
            const passwordField = $('#novaSenhaRedefinir');
            const type = passwordField.attr('type') === 'password' ? 'text' : 'password';
            passwordField.attr('type', type);
            $(this).find('i').toggleClass('fa-eye fa-eye-slash');
        });

        $('#toggleConfirmarSenhaRedefinir').on('click', function () {
            const confirmPasswordField = $('#confirmarSenhaRedefinir');
            const type = confirmPasswordField.attr('type') === 'password' ? 'text' : 'password';
            confirmPasswordField.attr('type', type);
            $(this).find('i').toggleClass('fa-eye fa-eye-slash');
        });

        // Função para verificar se as senhas coincidem
        $('#confirmarSenhaRedefinir').on('input', function () {
            const novaSenha = $('#novaSenhaRedefinir').val();
            const confirmarSenha = $(this).val();

            // Verifica se as senhas coincidem
            if (novaSenha !== confirmarSenha) {
                $('#errorMessageRedefinir').text('As senhas não coincidem.').show();
                $('#redefinirSenhaForm button[type="submit"]').prop('disabled', true);
            } else {
                $('#errorMessageRedefinir').hide();
                if (novaSenha.length >= 8 && /[A-Z]/.test(novaSenha) && /[a-z]/.test(novaSenha) && /\d/.test(novaSenha) && /[!@#$%^&*]/.test(novaSenha)) {
                    $('#redefinirSenhaForm button[type="submit"]').prop('disabled', false); // Habilita o botão
                } else {
                    $('#redefinirSenhaForm button[type="submit"]').prop('disabled', true); // Desabilita o botão
                }
            }
        });

        $('#redefinirSenhaForm').on('submit', function (e) {
            e.preventDefault();

            const clienteId = $('#clienteId').val();
            const token = $('#token').val();
            const novaSenha = $('#novaSenhaRedefinir').val();
            const confirmarSenha = $('#confirmarSenhaRedefinir').val();
            const barbeariaUrl = $('#barbeariaUrl').val(); // Captura a URL da barbearia

            // Verifica se as senhas coincidem
            if (novaSenha !== confirmarSenha) {
                $('#errorMessageRedefinir').text('As senhas não coincidem.').show();
                return;
            } else {
                $('#errorMessageRedefinir').hide();
            }

            // Envia a nova senha para o servidor
            $.ajax({
                url: '/Login/RedefinirSenha',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    clienteId: parseInt(clienteId, 10), // Converte para número
                    token: token,
                    novaSenha: novaSenha
                }),
                success: function (data) {
                    if (data.success) {
                        // Exibe o toast de sucesso com a mensagem de redirecionamento
                        showToast('Senha redefinida com sucesso! Redirecionando para a tela inicial...', 'success');

                        // Espera 5 segundos (tempo de exibição do toast) e depois redireciona para a URL fornecida pelo servidor
                        setTimeout(function () {
                            window.location.href = data.redirectUrl; // Redireciona para a URL correta da barbearia
                        }, 5000); // Atraso de 5 segundos
                    } else {
                        // Exibe o toast de erro
                        showToast(data.message || 'Erro ao redefinir senha.', 'danger');
                    }
                },
                error: function () {
                    // Exibe o toast de erro caso haja erro no processamento
                    showToast('Erro ao processar a solicitação.', 'danger');
                }
            });
        });

    }

    var redefinirSenhaAdminPage = document.getElementById('redefinirSenhaPageAdmin');

    if (redefinirSenhaAdminPage) {
        $('#novaSenhaRedefinirAdmin').on('input', function () {
            const password = $(this).val();
            const lengthRequirement = password.length >= 8;
            const uppercaseRequirement = /[A-Z]/.test(password);
            const lowercaseRequirement = /[a-z]/.test(password);
            const numberRequirement = /\d/.test(password);
            const specialRequirement = /[!@#$%^&*]/.test(password);

            $('#lengthRequirementRedefinirAdmin').toggleClass('text-success', lengthRequirement).toggleClass('text-danger', !lengthRequirement);
            $('#uppercaseRequirementRedefinirAdmin').toggleClass('text-success', uppercaseRequirement).toggleClass('text-danger', !uppercaseRequirement);
            $('#lowercaseRequirementRedefinirAdmin').toggleClass('text-success', lowercaseRequirement).toggleClass('text-danger', !lowercaseRequirement);
            $('#numberRequirementRedefinirAdmin').toggleClass('text-success', numberRequirement).toggleClass('text-danger', !numberRequirement);
            $('#specialRequirementRedefinirAdmin').toggleClass('text-success', specialRequirement).toggleClass('text-danger', !specialRequirement);
        });

        // Toggle de visibilidade das senhas
        $('#toggleNovaSenhaRedefinirAdmin').on('click', function () {
            const passwordField = $('#novaSenhaRedefinirAdmin');
            const type = passwordField.attr('type') === 'password' ? 'text' : 'password';
            passwordField.attr('type', type);
            $(this).find('i').toggleClass('fa-eye fa-eye-slash');
        });

        $('#toggleConfirmarSenhaRedefinirAdmin').on('click', function () {
            const confirmPasswordField = $('#confirmarSenhaRedefinirAdmin');
            const type = confirmPasswordField.attr('type') === 'password' ? 'text' : 'password';
            confirmPasswordField.attr('type', type);
            $(this).find('i').toggleClass('fa-eye fa-eye-slash');
        });

        // Função para verificar se as senhas coincidem
        $('#confirmarSenhaRedefinirAdmin').on('input', function () {
            const novaSenha = $('#novaSenhaRedefinirAdmin').val();
            const confirmarSenha = $(this).val();

            // Verifica se as senhas coincidem
            if (novaSenha !== confirmarSenha) {
                $('#errorMessageRedefinirAdmin').text('As senhas não coincidem.').show();
                $('#redefinirSenhaFormAdmin button[type="submit"]').prop('disabled', true); // Desabilita o botão
            } else {
                $('#errorMessageRedefinirAdmin').hide();
                if (novaSenha.length >= 8 && /[A-Z]/.test(novaSenha) && /[a-z]/.test(novaSenha) && /\d/.test(novaSenha) && /[!@#$%^&*]/.test(novaSenha)) {
                    $('#redefinirSenhaFormAdmin button[type="submit"]').prop('disabled', false); // Habilita o botão
                } else {
                    $('#redefinirSenhaFormAdmin button[type="submit"]').prop('disabled', true); // Desabilita o botão
                }
            }
        });

        $('#redefinirSenhaFormAdmin').on('submit', function (e) {
            e.preventDefault();

            const usuarioId = $('#usuarioIdAdmin').val();
            const token = $('#tokenAdmin').val();
            const novaSenha = $('#novaSenhaRedefinirAdmin').val();
            const confirmarSenha = $('#confirmarSenhaRedefinirAdmin').val();

            // Verifica se as senhas coincidem
            if (novaSenha !== confirmarSenha) {
                $('#errorMessageRedefinirAdmin').text('As senhas não coincidem.').show();
                return;
            } else {
                $('#errorMessageRedefinirAdmin').hide();
            }

            // Envia a nova senha para o servidor
            $.ajax({
                url: '/Login/RedefinirSenhaAdmin',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    clienteId: parseInt(usuarioId, 10), // Converte para número
                    token: token,
                    novaSenha: novaSenha
                }),
                success: function (data) {
                    if (data.success) {
                        // Exibe o toast de sucesso com a mensagem de redirecionamento
                        showToast('Senha redefinida com sucesso! Redirecionando para a tela inicial...', 'success');

                        // Espera 5 segundos (tempo de exibição do toast) e depois redireciona para a URL fornecida pelo servidor
                        setTimeout(function () {
                            window.location.href = data.redirectUrl; // Redireciona para a URL correta da barbearia
                        }, 5000); // Atraso de 5 segundos
                    } else {
                        // Exibe o toast de erro
                        showToast(data.message || 'Erro ao redefinir senha.', 'danger');
                    }
                },
                error: function () {
                    // Exibe o toast de erro caso haja erro no processamento
                    showToast('Erro ao processar a solicitação.', 'danger');
                }
            });
        });
    }



    // Verifica se a div "pagineInicialIndex" está presente no DOM
    const paginaInicialIndex = document.getElementById("pagineInicialIndex");

    if (paginaInicialIndex) {

        const searchBar = document.getElementById("searchBarbearia");
        const barbeariaCards = document.querySelectorAll(".barbearia-card-container");

        // Ativa a funcionalidade de busca ao vivo se o campo de busca estiver presente
        if (searchBar && barbeariaCards.length > 0) {
            searchBar.addEventListener("input", function () {
                const query = this.value.toLowerCase();
                barbeariaCards.forEach(card => {
                    const name = card.getAttribute("data-name");
                    if (name.includes(query)) {
                        card.style.display = "block";
                    } else {
                        card.style.display = "none";
                    }
                });
            });
        }
    }

    var alterarSenhaPageCliente = document.getElementById('alterarSenhaPageCliente');

    if (alterarSenhaPageCliente) {
        // Validação de força da nova senha
        $('#novaSenhaCliente').on('input', function () {
            const password = $(this).val();
            const lengthRequirement = password.length >= 8;
            const uppercaseRequirement = /[A-Z]/.test(password);
            const lowercaseRequirement = /[a-z]/.test(password);
            const numberRequirement = /\d/.test(password);
            const specialRequirement = /[!@#$%^&*]/.test(password);

            $('#lengthRequirementAlterarSenhaCliente').toggleClass('text-success', lengthRequirement).toggleClass('text-danger', !lengthRequirement);
            $('#uppercaseRequirementAlterarSenhaCliente').toggleClass('text-success', uppercaseRequirement).toggleClass('text-danger', !uppercaseRequirement);
            $('#lowercaseRequirementAlterarSenhaCliente').toggleClass('text-success', lowercaseRequirement).toggleClass('text-danger', !lowercaseRequirement);
            $('#numberRequirementAlterarSenhaCliente').toggleClass('text-success', numberRequirement).toggleClass('text-danger', !numberRequirement);
            $('#specialRequirementAlterarSenhaCliente').toggleClass('text-success', specialRequirement).toggleClass('text-danger', !specialRequirement);
        });

        // Exibir ou ocultar senha
        $('#toggleSenhaAtualCliente, #toggleNovaSenhaCliente').on('click', function () {
            const input = $(this).prev('input');
            const type = input.attr('type') === 'password' ? 'text' : 'password';
            input.attr('type', type);
            $(this).find('i').toggleClass('fa-eye fa-eye-slash');
        });

        // Submissão do formulário
        $('#alterarSenhaFormCliente').on('submit', function (e) {
            e.preventDefault();

            const clienteId = $('#clienteIdCliente').val();
            const senhaAtual = $('#senhaAtualCliente').val();
            const novaSenha = $('#novaSenhaCliente').val();

            $.ajax({
                url: '/Cliente/AlterarSenhaCliente',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    senhaAtual: senhaAtual,
                    novaSenha: novaSenha
                }),
                success: function (data) {
                    if (data.success) {
                        showToast('Senha alterada com sucesso!', 'success');
                        setTimeout(() => window.location.href = '/Cliente/MenuPrincipal', 3000);
                    } else {
                        showToast(data.message || 'Erro ao alterar senha.', 'danger');
                    }
                },
                error: function () {
                    showToast('Erro ao processar a solicitação.', 'danger');
                }
            });
        });
    }

    const meusDadosClientePage = document.getElementById('meusdadosClientePage');

    if (meusDadosClientePage) {
        const nomeInput = document.getElementById('nome');
        const emailInput = document.getElementById('email');
        const telefoneInput = document.getElementById('telefone');
        const form = meusDadosClientePage.querySelector('form');

        // Máscara de Telefone ((xx) xxxxx-xxxx)
        telefoneInput.addEventListener('input', function () {
            let telefone = this.value.replace(/\D/g, ''); // Remove caracteres não numéricos
            if (telefone.length > 2) {
                telefone = '(' + telefone.slice(0, 2) + ') ' + telefone.slice(2);
            }
            if (telefone.length > 9) {
                telefone = telefone.slice(0, 9) + '-' + telefone.slice(9, 13); // Formato (xx) xxxxx-xxxx
            }
            this.value = telefone;
        });

        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const clienteData = {
                ClienteId: parseInt(document.querySelector('#clienteId')?.value || 0), // Inclua o ClienteId se necessário
                Nome: nomeInput.value.trim(),
                Email: emailInput.value.trim(),
                Telefone: telefoneInput.value.trim(),
            };

            // Validação básica
            if (!clienteData.Nome || !clienteData.Email || !clienteData.Telefone) {
                showToast('Por favor, preencha todos os campos corretamente.', 'danger');
                return;
            }

            // Requisição Ajax para salvar os dados
            fetch('/Cliente/MeusDadosCliente', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(clienteData),
            })
                .then((response) => response.json())
                .then((data) => {
                    if (data.success) {
                        showToast(data.message, 'success');
                    } else {
                        showToast(data.message, 'danger');
                    }
                })
                .catch((error) => {
                    showToast('Erro ao salvar os dados. Tente novamente.', 'danger');
                    console.error('Erro:', error);
                });
        });
    }



    // Função para exibir Toasts com setTimeout
    function showToast(message, type = 'info') {

        const toastHtml = `
    <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    </div>`;

        // Adiciona o toast ao container de toasts
        $('#toastContainer').append(toastHtml);
        const toastElement = $('#toastContainer .toast').last();

        // Cria a instância do Toast
        const toastInstance = new bootstrap.Toast(toastElement[0]);

        // Exibe o toast
        toastInstance.show();

        // Usando setTimeout para esperar 5 segundos antes de remover o toast
        setTimeout(function () {
            // Fecha o toast manualmente após 5 segundos
            toastInstance.hide();

            // Remove o toast do DOM após o fechamento
            toastElement.remove();
        }, 5000); // Atraso de 5 segundos
    }


    // Variáveis globais
    var emailDomains = ["gmail.com", "yahoo.com.br", "outlook.com", "hotmail.com"];

    // Função para validar email
    function isValidEmail(email) {
        var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    // Função para validar telefone
    function isValidPhone(phone) {
        var regex = /^\(\d{2}\)\s\d{5}-\d{4}$/;
        return regex.test(phone);
    }

    // Autocomplete e máscara de telefone
    $('#inputFieldLogin').on('input', function () {
        var inputValue = $(this).val();

        if (/^[a-zA-Z0-9._%+-]+@?$/.test(inputValue)) { // Checa se é email
            if (inputValue.includes('@') && inputValue.indexOf('@') === inputValue.length - 1) {
                var dropdownHtml = '';
                emailDomains.forEach(function (domain) {
                    dropdownHtml += '<div class="autocomplete-suggestion">' + inputValue + domain + '</div>';
                });
                $('#emailAutocompleteLogin').html(dropdownHtml).fadeIn();
            } else {
                $('#emailAutocompleteLogin').fadeOut();
            }
        } else if (/^\d+$/.test(inputValue)) { // Checa se é telefone
            $(this).val(inputValue.replace(/^(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3'));
            $('#emailAutocompleteLogin').fadeOut();
        } else {
            $('#emailAutocompleteLogin').fadeOut();
        }
    });

    $(document).on('click', '.autocomplete-suggestion', function () {
        $('#inputFieldLogin').val($(this).text());
        $('#emailAutocompleteLogin').fadeOut();
    });

    // Alternar visibilidade da senha no login
    $('#toggleLoginPassword').on('click', function () {
        var passwordInput = $('#passwordInputLogin');
        var icon = $('#toggleLoginPassword i');

        if (passwordInput.attr("type") === "password") {
            passwordInput.attr("type", "text");
            icon.removeClass("fa-eye").addClass("fa-eye-slash");
        } else {
            passwordInput.attr("type", "password");
            icon.removeClass("fa-eye-slash").addClass("fa-eye");
        }
    });

    // Submissão do formulário de login via AJAX
    $('#loginForm').on('submit', function (e) {
        e.preventDefault();

        var inputValue = $('#inputFieldLogin').val().trim();
        var passwordValue = $('#passwordInputLogin').val().trim();

        // Verifica se os campos estão preenchidos corretamente
        if (!inputValue || !passwordValue) {
            showToast('Por favor, insira um telefone, email e senha válidos.', 'danger');
            return;
        }

        $('#loadingSpinner').fadeIn(); // Mostra o spinner de tela cheia
        $('#submitButtonLogin').prop('disabled', true);
        var formData = $(this).serialize();

        $.ajax({
            type: 'POST',
            url: '/Login/Login',
            data: formData,
            success: function (data) {
                $('#loadingSpinner').fadeOut(); // Esconde o spinner
                $('#submitButtonLogin').prop('disabled', false);

                if (data.success) {
                    window.location.href = data.redirectUrl;
                } else {
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                $('#loadingSpinner').fadeOut(); // Esconde o spinner
                $('#submitButtonLogin').prop('disabled', false);
                showToast('Ocorreu um erro. Por favor, tente novamente.', 'danger');
            }
        });
    });






    // Função para aplicar máscara no número de telefone em tempo real
    $('#registerPhoneInput').on('input', function () {
        var inputValue = $(this).val().replace(/\D/g, ''); // Remove todos os caracteres não numéricos
        if (inputValue.length > 0) {
            inputValue = inputValue.replace(/^(\d{2})(\d{5})(\d{0,4}).*/, '($1) $2-$3');
        }
        $(this).val(inputValue);
    });

    // Função para verificar se o input é um e-mail e sugerir autocompletar
    $('#registerEmailInput').on('input', function () {
        var inputValue = $(this).val();

        if (inputValue.includes('@') && inputValue.indexOf('@') === inputValue.length - 1) {
            var dropdownHtml = '';
            emailDomains.forEach(function (domain) {
                dropdownHtml += '<div class="autocomplete-suggestion">' + inputValue + domain + '</div>';
            });
            $('#registerEmailAutocomplete').html(dropdownHtml).fadeIn();
        } else {
            $('#registerEmailAutocomplete').fadeOut();
        }
    });

    $(document).on('click', '.autocomplete-suggestion', function () {
        $('#registerEmailInput').val($(this).text());
        $('#registerEmailAutocomplete').fadeOut();
    });

    // Função de validação de senha
    $('#registerPasswordInput').on('input', function () {
        var password = $(this).val();
        var lengthRequirement = password.length >= 8;
        var uppercaseRequirement = /[A-Z]/.test(password);
        var lowercaseRequirement = /[a-z]/.test(password);
        var numberRequirement = /\d/.test(password);
        var specialRequirement = /[!@#$%^&*]/.test(password);

        $('#lengthRequirement').toggleClass('text-success', lengthRequirement).toggleClass('text-danger', !lengthRequirement);
        $('#uppercaseRequirement').toggleClass('text-success', uppercaseRequirement).toggleClass('text-danger', !uppercaseRequirement);
        $('#lowercaseRequirement').toggleClass('text-success', lowercaseRequirement).toggleClass('text-danger', !lowercaseRequirement);
        $('#numberRequirement').toggleClass('text-success', numberRequirement).toggleClass('text-danger', !numberRequirement);
        $('#specialRequirement').toggleClass('text-success', specialRequirement).toggleClass('text-danger', !specialRequirement);

        checkFormValidity();
    });

    // Função para alternar a visibilidade da senha
    function togglePasswordVisibility(inputId, toggleIconId) {
        var input = $(inputId);
        var icon = $(toggleIconId);
        if (input.attr("type") === "password") {
            input.attr("type", "text");
            icon.removeClass("fa-eye").addClass("fa-eye-slash");
        } else {
            input.attr("type", "password");
            icon.removeClass("fa-eye-slash").addClass("fa-eye");
        }
    }

    // Alternar visibilidade para campo de senha principal
    $('#toggleRegisterPassword').on('click', function () {
        togglePasswordVisibility('#registerPasswordInput', '#toggleRegisterPassword i');
    });

    // Alternar visibilidade para campo de confirmação de senha
    $('#toggleConfirmPassword').on('click', function () {
        togglePasswordVisibility('#confirmPasswordInput', '#toggleConfirmPassword i');
    });

    // Função para verificar se as senhas coincidem
    function checkPasswordMatch() {
        var password = $('#registerPasswordInput').val();
        var confirmPassword = $('#confirmPasswordInput').val();

        if (password !== confirmPassword) {
            $('#passwordMatchError').show();
            $('#confirmPasswordInput').addClass('is-invalid').removeClass('is-valid');
            $('#checkIcon').hide(); // Esconder o ícone de "check" se não coincidir
        } else {
            $('#passwordMatchError').hide();
            $('#confirmPasswordInput').removeClass('is-invalid').addClass('is-valid');
            $('#checkIcon').show(); // Mostrar o ícone de "check" se coincidir
        }
        checkFormValidity();
    }

    // Verificação em tempo real para confirmar senha
    $('#confirmPasswordInput').on('input', checkPasswordMatch);

    // Função para verificar a validade do formulário
    function checkFormValidity() {
        var name = $('#nameInput').val().trim();
        var email = $('#registerEmailInput').val().trim();
        var phone = $('#registerPhoneInput').val().trim();
        var password = $('#registerPasswordInput').val();
        var confirmPassword = $('#confirmPasswordInput').val();

        var isPasswordValid = password.length >= 8 &&
            /[A-Z]/.test(password) &&
            /[a-z]/.test(password) &&
            /\d/.test(password) &&
            /[!@#$%^&*]/.test(password);

        var isFormValid = name !== "" && email !== "" && phone !== "" &&
            isPasswordValid && password === confirmPassword;

        $('#registerForm button[type="submit"]').prop('disabled', !isFormValid);
    }

    // Desabilitar o botão de cadastrar inicialmente
    $('#registerForm button[type="submit"]').prop('disabled', true);

    // Mostrar toast de erro
    //function showToast(message, type) {
    //    $('#registerErrorMessage').text(message).fadeIn();
    //    setTimeout(function () {
    //        $('#registerErrorMessage').fadeOut();
    //    }, 3000);
    //}

    // Validação do formulário de cadastro
    // Adiciona console logs para verificar valores antes de enviar o formulário
    $('#registerForm').on('submit', function (e) {
        e.preventDefault();

        var isFormValid = !$('#registerForm button[type="submit"]').prop('disabled');

        if (!isFormValid) {
            showToast("Por favor, preencha todos os campos corretamente.", 'danger');
            return;
        }

        $('#loadingSpinner').fadeIn();
        var formData = $(this).serialize();

        $.ajax({
            type: 'POST',
            url: '/Login/Cadastro',
            data: formData,
            success: function (data) {
                $('#loadingSpinner').fadeOut();
                if (data.success) {
                    $('#registerModal').modal('hide');
                    window.location.href = data.redirectUrl; // Redireciona diretamente para a URL fornecida
                } else {
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                $('#loadingSpinner').fadeOut();
                showToast('Erro ao registrar. Por favor, tente novamente.', 'danger');
            }
        });
    });


    // Verificar a validade do formulário a cada mudança nos inputs
    $('#nameInput, #registerEmailInput, #registerPhoneInput, #registerPasswordInput, #confirmPasswordInput').on('input', checkFormValidity);

    $('#verificationForm').on('submit', function (e) {
        e.preventDefault();
        var clienteId = $('#clienteIdField').val();
        var formData = $(this).serialize();

        $.ajax({
            type: 'POST',
            url: '/Login/VerificarCodigo',
            data: formData,
            success: function (data) {
                if (data.success) {
                    clearInterval(countdownInterval); // Parar o contador
                    window.location.href = data.redirectUrl;
                } else {
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                showToast('Ocorreu um erro. Tente novamente.', 'danger');
            }
        });
    });

    // Reenvio do código de verificação via AJAX
    $('#resendCode').on('click', function (e) {
        e.preventDefault();
        var clienteId = $('#clienteIdField').val();

        $.ajax({
            type: 'GET',
            url: '/Login/ReenviarCodigo',
            data: { clienteId: clienteId },
            success: function (data) {
                if (data.success) {
                    showToast('Novo código enviado.', 'success');
                    resetCountdown();
                } else {
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                showToast('Erro ao reenviar o código. Tente novamente.', 'danger');
            }
        });
    });

    // Funções do contador
    function startCountdown() {
        var timeLeft = countdownTime;
        $('#countdownText').html('Você poderá solicitar um novo código em: <span id="countdownTimer">' + timeLeft + '</span> segundos.');
        $('#resendCodeLink').hide();

        countdownInterval = setInterval(function () {
            timeLeft--;
            $('#countdownTimer').text(timeLeft);

            if (timeLeft <= 0) {
                clearInterval(countdownInterval);
                $('#countdownText').text('Clique em reenviar para solicitar um novo código.');
                $('#resendCodeLink').show();
            }
        }, 1000);
    }

    function resetCountdown() {
        clearInterval(countdownInterval);
        startCountdown();
    }

    function resetCountdown() {
        clearInterval(countdownInterval);
        startCountdown();
    }

    if ($('#loginErrorToast').length > 0) {
        var toastEl = new bootstrap.Toast(document.getElementById('loginErrorToast'));
        toastEl.show();
    }

    // Lógica do menuPrincipal
    if ($('#menuPrincipal').length > 0) {
        $('#historicoButton').on('click', function () {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);
            window.location.href = '/Agendamento/Historico';
        });

        $('#servicoButton').on('click', function () {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);
            window.location.href = '/Cliente/SolicitarServico';
        });
    }

    // Lógica para a página de Solicitar Serviço
    if ($('#solicitarServicoPage').length > 0) {
        var servicosSelecionados = [];
        var valorTotal = 0;
        var duracaoTotal = 0;

        window.adicionarServico = function (id, nome, preco, duracao, element) {
            preco = preco.replace(',', '.');
            var index = servicosSelecionados.findIndex(servico => servico.id === id);

            if (index === -1) {
                servicosSelecionados.push({ id, nome, preco, duracao });
                valorTotal += parseFloat(preco);
                duracaoTotal += parseInt(duracao);
                $(element).prop('disabled', true);
            }
            atualizarListaServicosSelecionados();
        };

        window.removerServico = function (index, id) {
            valorTotal -= parseFloat(servicosSelecionados[index].preco.toString().replace(',', '.'));
            duracaoTotal -= parseInt(servicosSelecionados[index].duracao);
            servicosSelecionados.splice(index, 1);
            $('#servico-' + id).prop('disabled', false);
            atualizarListaServicosSelecionados();
        };

        function atualizarListaServicosSelecionados() {
            var lista = $('#servicosSelecionados');
            lista.empty();
            servicosSelecionados.forEach(function (servico, index) {
                lista.append(
                    `<li class="list-group-item d-flex justify-content-between align-items-center">
                    <span>${servico.nome} - R$ ${parseFloat(servico.preco.replace(',', '.')).toFixed(2)}</span>
                    <button class="btn btn-danger btn-sm" onclick="removerServico(${index}, '${servico.id}')">Remover</button>
                </li>`
                );
            });
            $('#valorTotal').text(valorTotal.toFixed(2));
        }

        window.confirmarServico = function () {
            if (servicosSelecionados.length === 0) {
                showToast('Nenhum serviço selecionado.', 'danger');
                return;
            }

            var servicoIds = servicosSelecionados.map(s => s.id);
            $('#loadingSpinner').fadeIn();
            sessionStorage.setItem('servicosSelecionados', JSON.stringify(servicosSelecionados));

            var barbeariaUrl = $('#barbeariaUrl').val();
            var barbeariaId = $('#barbeariaId').val();

            window.location.href = `/${barbeariaUrl}/Barbeiro/EscolherBarbeiro?duracaoTotal=${duracaoTotal}&servicoIds=${servicoIds.join(',')}&barbeariaId=${barbeariaId}`;
        };

        var servicosArmazenados = JSON.parse(sessionStorage.getItem('servicosSelecionados')) || [];
        servicosArmazenados.forEach(function (servico) {
            servicosSelecionados.push(servico);
            valorTotal += parseFloat(servico.preco.toString().replace(',', '.'));
            duracaoTotal += parseInt(servico.duracao);
            $('#servico-' + servico.id).prop('disabled', true);
        });
        atualizarListaServicosSelecionados();
    }




    // Lógica para a página de Escolher Barbeiro
    if ($('#escolherBarbeiroPage').length > 0) {


        // Seleciona todos os cards
        const cards = document.querySelectorAll(".barber-card");

        if (cards.length > 0) {
            let maxHeight = 0;
            let maxServicesHeight = 0;

            // Calcula a maior altura do card e a maior altura da lista de serviços
            cards.forEach((card) => {
                const cardHeight = card.scrollHeight;
                const servicesContainer = card.querySelector(".services-container");

                // Verifica a altura do card
                if (cardHeight > maxHeight) {
                    maxHeight = cardHeight;
                }

                // Verifica a altura do contêiner de serviços
                if (servicesContainer && servicesContainer.scrollHeight > maxServicesHeight) {
                    maxServicesHeight = servicesContainer.scrollHeight;
                }
            });

            console.log("Maior altura do card:", maxHeight);
            console.log("Maior altura da lista de serviços:", maxServicesHeight);

            // Aplica a altura máxima para todos os cards e ajusta a posição do botão
            cards.forEach((card) => {
                card.style.height = `${maxHeight}px`; // Define a altura do card

                const servicesContainer = card.querySelector(".services-container");
                const button = card.querySelector(".btn-danger");

                // Ajusta a altura do contêiner de serviços e a posição do botão
                if (servicesContainer) {
                    servicesContainer.style.height = `${maxServicesHeight}px`;
                }

                if (button) {
                    button.style.marginTop = "auto"; // Garante que o botão fique ao final
                }
            });
        }

        // Carregar a localização em português para o Flatpickr
        flatpickr.localize(flatpickr.l10ns.pt);
        var selectedBarbeiroId = null;
        var selectedDuracaoTotal = $('#escolherBarbeiroPage').data('duracao-total');
        var selectedServicoIds = $('#escolherBarbeiroPage').data('servico-ids');
        var selectedDate = null; // Armazena a data selecionada
        var horarioSelecionado = null; // Armazena o horário selecionado
        var horariosPorDia = {}; // Objeto para armazenar os horários disponíveis por dia


        // Evento de clique no botão de cada barbeiro
        $('.barbeiro-btn').on('click', function () {
            selectedBarbeiroId = $(this).data('barbeiro-id');

            if (!selectedDuracaoTotal || selectedDuracaoTotal <= 0) {
                showToast("Nenhum serviço selecionado ou duração inválida.", "danger");
                return;
            }

            $('#calendarioModal').modal('show'); // Abre o modal do calendário
            carregarDiasDisponiveis(); // Carrega os dias e horários disponíveis
        });

        function carregarDiasDisponiveis() {
            const barbeariaUrl = $('#barbeariaUrl').val();

            $.ajax({
                url: `/${barbeariaUrl}/Agendamento/ObterHorariosDisponiveis`,
                data: {
                    barbeiroId: selectedBarbeiroId,
                    duracaoTotal: selectedDuracaoTotal
                },
                success: function (data) {
                    console.log("Horários recebidos do backend:", data); // Log dos horários recebidos do backend

                    horariosPorDia = {}; // Limpa o objeto antes de preencher

                    data.forEach(function (horario) {
                        const dataHora = dayjs(horario).add(3, 'hour'); // Ajuste para UTC+3
                        const dia = dataHora.format('YYYY-MM-DD');
                        const horarioInicio = dataHora.format('HH:mm');
                        const horarioFim = dataHora.add(selectedDuracaoTotal, 'minute').format('HH:mm');

                        // Log para verificar cada horário processado com ajuste adicional
                        console.log(`Processando horário ajustado: ${dataHora} (Início: ${horarioInicio} - Fim: ${horarioFim})`);

                        if (!horariosPorDia[dia]) {
                            horariosPorDia[dia] = []; // Cria o array de horários para o dia
                        }
                        horariosPorDia[dia].push(`${horarioInicio} - ${horarioFim}`); // Adiciona o horário formatado ao dia
                    });

                    // Log para verificar como ficou o objeto final
                    console.log("Horários organizados por dia após ajuste:", horariosPorDia);

                    configurarCalendario(Object.keys(horariosPorDia));
                },
                error: function () {
                    showToast('Erro ao carregar os horários.', "danger");
                }
            });
        }




        function configurarCalendario(diasDisponiveis) {
            flatpickr("#calendarioInput", {
                inline: true,
                dateFormat: "Y-m-d",
                locale: "pt", // Define o calendário para o português
                enable: diasDisponiveis, // Somente dias disponíveis são habilitados
                disableMobile: true, // Garante que o Flatpickr customizado apareça em dispositivos móveis
                onChange: function (selectedDates, dateStr, instance) {
                    selectedDate = dateStr;
                    $('#calendarioModal').modal('hide'); // Fecha o modal do calendário
                    mostrarHorariosParaDia(selectedDate); // Mostra os horários para o dia selecionado
                },
                onDayCreate: function (dObj, dStr, fp, dayElem) {
                    // Verifica se o dia está disponível
                    const isAvailable = diasDisponiveis.includes(dayElem.dateObj.toISOString().split("T")[0]);
                    if (!isAvailable) {
                        // Se o dia não estiver disponível, adiciona evento de clique para mostrar o toast
                        dayElem.classList.add("disabled");
                        dayElem.addEventListener("click", function (e) {
                            e.preventDefault();
                            showToast("Data não disponível", "warning"); // Exibe o toast
                        });
                    }
                }
            });
        }


        // Função para exibir os horários disponíveis para o dia selecionado
        function mostrarHorariosParaDia(diaSelecionado) {
            // Formata o título com a data no formato "Horários disponíveis em [dia] de [mês] de [ano]"
            const dataFormatada = dayjs(diaSelecionado).format('DD [de] MMMM [de] YYYY');
            $('#horariosModalLabel').text(`Horários disponíveis em ${dataFormatada}`);

            $('#horariosContainer').empty(); // Limpa os horários anteriores

            if (horariosPorDia[diaSelecionado]) {
                horariosPorDia[diaSelecionado].forEach(function (horario) {
                    // Cria um botão para cada horário disponível no formato "início - fim"
                    let horarioBtn = `<button class="btn btn-outline-light m-1 horario-btn" data-horario="${horario}">${horario}</button>`;
                    $('#horariosContainer').append(horarioBtn);
                });
            } else {
                $('#horariosContainer').append('<p class="text-light">Nenhum horário disponível para este dia.</p>');
            }

            $('#horariosModal').modal('show'); // Abre o modal de horários
        }

        // Evento de clique para cada horário disponível
        $(document).on('click', '.horario-btn', function () {
            horarioSelecionado = $(this).data('horario');

            if (!horarioSelecionado) {
                showToast('Por favor, selecione um horário.', "warning");
                return;
            }

            $('#loadingSpinner').fadeIn();

            sessionStorage.removeItem('servicosSelecionados');

            const barbeariaUrl = $('#barbeariaUrl').val(); // Obtém o barbeariaUrl
            const barbeariaId = $('#barbeariaId').val();   // Obtém o barbeariaId

            // Redireciona para a página de resumo com os dados selecionados
            window.location.href = `/${barbeariaUrl}/Agendamento/ResumoAgendamento?barbeiroId=${selectedBarbeiroId}&dataHora=${encodeURIComponent(selectedDate + ' ' + horarioSelecionado.split(' - ')[0])}&servicoIds=${selectedServicoIds}&barbeariaId=${barbeariaId}`;
        });

        // Evento para o botão "Voltar" no modal de horários
        $(document).on('click', '#voltarParaCalendarioBtn', function () {
            $('#horariosModal').modal('hide'); // Fecha o modal de horários
            $('#calendarioModal').modal('show'); // Reabre o modal de calendário
        });

        $('#voltarBtn').on('click', function () {
            window.location.href = '/Cliente/SolicitarServico';
        });
    }




    if ($('#resumoAgendamentoPage').length > 0) {
        let selectedPaymentMethod = null;
        const stripe = Stripe("pk_live_51QFMA5Hl3zYZjP9pt96kFgHES5ArjpXgdXa2AXrZr3IXsqpWC9JpHAsLajdeOMCIoCu31wruWj5SLeqbmh9aeVPU003NEkIbJe");
        let elements = null;
        let cardElement = null;
        let clientSecret = null;
        let agendamentoId = null;

        const appearance = {
            theme: 'stripe',
            variables: {
                colorPrimary: '#007bff',
                colorBackground: '#1a1a2e',
                colorText: '#ffffff',
                colorTextSecondary: '#ffffff',
                colorTextPlaceholder: '#ffffff',
                colorDanger: '#ff3860',
                fontFamily: 'Arial, sans-serif',
                spacingUnit: '4px',
                borderRadius: '4px'
            }
        };

        //async function initializeCardElement() {
        //    try {
        //        const amount = parseFloat($('#total-price').data('preco-total'));
        //        const barbeariaId = $('#barbeariaId').val();
        //        const commissionPercentage = 0.10; // Substitua pelo valor da comissão desejado
        //        const currency = 'brl'; // Defina a moeda desejada, se necessário

        //        const response = await $.ajax({
        //            url: '/api/payment/create-payment-intent-barbearia',
        //            type: 'POST',
        //            contentType: 'application/json',
        //            data: JSON.stringify({
        //                amount: amount,
        //                barbeariaId: barbeariaId,
        //                paymentMethods: ['card'],
        //                currency: currency,
        //                commissionPercentage: commissionPercentage
        //            }),
        //            dataType: 'json'
        //        });

        //        clientSecret = response.clientSecret;
        //        elements = stripe.elements({ appearance });
        //        cardElement = elements.create('card');
        //        cardElement.mount('#payment-element');
        //    } catch (error) {
        //        console.error('Erro ao inicializar o elemento do cartão:', error);
        //        showToast('Erro ao configurar o pagamento. Tente novamente mais tarde.', 'danger');
        //    }
        //}

        async function initializeCardElement() {
            try {
                const amount = parseFloat($('#total-price').data('preco-total'));

                const response = await $.ajax({
                    url: '/api/payment/create-payment-intent',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ amount: amount }),
                    dataType: 'json'
                });

                clientSecret = response.clientSecret;
                elements = stripe.elements({ appearance });
                cardElement = elements.create('card');
                cardElement.mount('#payment-element');
            } catch (error) {
                console.error('Erro ao inicializar o elemento do cartão:', error);
                showToast('Erro ao configurar o pagamento. Tente novamente mais tarde.', 'danger');
            }
        }


        function selectPaymentMethod(method) {
            selectedPaymentMethod = method;
            $('.payment-option').removeClass('selected').hide();
            $('#' + method + 'Option').addClass('selected').show();
            $('#payment-form, #confirmarAgendamentoBtn').hide();

            if (method === 'creditCard') {
                $('#payment-form').fadeIn();
                initializeCardElement();
            } else if (method === 'store') {
                $('#confirmarAgendamentoBtn').fadeIn();
            }

            $('#changePaymentMethodBtn').show();
        }

        async function confirmarAgendamento() {
            const barbeiroId = $('#resumoAgendamentoPage').data('barbeiro-id');
            const dataHora = $('#resumoAgendamentoPage').data('data-hora');
            const servicoIds = $('#resumoAgendamentoPage').data('servico-ids');
            const barbeariaId = $('#barbeariaId').val(); // Obtendo o barbeariaId
            const barbeariaUrl = $('#barbeariaUrl').val(); // Obtendo o barbeariaUrl

            try {
                const response = await $.post(`/${barbeariaUrl}/Agendamento/ConfirmarAgendamento`, { // Incluindo barbeariaUrl na URL
                    barbeiroId,
                    dataHora,
                    servicoIds,
                    formaPagamento: selectedPaymentMethod,
                    barbeariaId,  // Incluindo barbeariaId no payload
                    barbeariaUrl  // Incluindo barbeariaUrl no payload
                });

                if (response.success) {
                    agendamentoId = response.agendamentoId;
                    return true;
                } else {
                    showToast(response.message || 'Erro ao confirmar agendamento.', 'danger');
                    $('#errorMessage').text(response.message || 'Erro ao confirmar agendamento. Tente novamente.');
                    $('#errorModal').modal('show');
                    $('#errorRedirectBtn').off('click').on('click', function () {
                        $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                    });
                    return false;
                }
            } catch (error) {
                console.error('Erro ao confirmar agendamento:', error);
                showToast('Erro ao confirmar o agendamento. Tente novamente mais tarde.', 'danger');
                $('#errorMessage').text('Erro ao confirmar o agendamento. Tente novamente mais tarde.');
                $('#errorModal').modal('show');
                $('#errorRedirectBtn').off('click').on('click', function () {
                    $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                });
                return false;
            }
        }

        async function processarPagamento() {
            $('#payment-form').submit();
        }

        $('#payment-form').on('submit', async function (e) {
            e.preventDefault();
            $('#loadingSpinner').fadeIn();

            try {
                if (selectedPaymentMethod === 'creditCard' && clientSecret) {
                    const clienteNome = $('input[name="clienteNome"]').val();
                    const clienteEmail = $('input[name="clienteEmail"]').val();

                    const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
                        payment_method: {
                            card: cardElement,
                            billing_details: {
                                name: clienteNome,
                                email: clienteEmail
                            }
                        }
                    });

                    if (error) {
                        console.error('Erro no pagamento:', error);
                        showToast('O pagamento não foi concluído. Entre em contato com a loja.', 'danger');
                        $('#errorMessage').text('O pagamento não foi concluído. Entre em contato com a barbearia para confirmar o agendamento.');
                        $('#errorModal').modal('show');
                        $('#errorRedirectBtn').off('click').on('click', function () {
                            window.location.href = '/Cliente/MenuPrincipal';
                        });
                    } else if (paymentIntent && paymentIntent.status === 'succeeded') {
                        await atualizarStatusPagamento(agendamentoId, "Aprovado", paymentIntent.id);
                    } else {
                        console.warn('Pagamento não concluído. Verifique os dados.');
                        showToast('O pagamento não foi concluído. Verifique os dados e tente novamente.', 'warning');
                        $('#errorMessage').text('O pagamento não foi concluído. Verifique os dados e tente novamente.');
                        $('#errorModal').modal('show');
                        $('#errorRedirectBtn').off('click').on('click', function () {
                            $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                        });
                    }
                } else {
                    showToast('Erro: clientSecret não definido.', 'danger');
                    $('#errorMessage').text('Erro: clientSecret não definido.');
                    $('#errorModal').modal('show');
                    $('#errorRedirectBtn').off('click').on('click', function () {
                        $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                    });
                }
            } catch (error) {
                console.error('Erro durante a confirmação do pagamento:', error);
                showToast('Erro ao confirmar o pagamento. Tente novamente mais tarde.', 'danger');
                $('#errorMessage').text('Erro ao confirmar o pagamento. Entre em contato com a barbearia para confirmar o agendamento.');
                $('#errorModal').modal('show');
                $('#errorRedirectBtn').off('click').on('click', function () {
                    window.location.href = '/Cliente/MenuPrincipal';
                });
            } finally {
                $('#loadingSpinner').fadeOut();
            }
        });

        async function atualizarStatusPagamento(agendamentoId, statusPagamento, paymentId) {
            const barbeariaId = $('#barbeariaId').val(); // Obtendo o barbeariaId
            const barbeariaUrl = $('#barbeariaUrl').val(); // Obtendo o barbeariaUrl

            try {
                const response = await $.post(`/${barbeariaUrl}/Agendamento/AtualizarStatusPagamento`, { // Incluindo barbeariaUrl na URL
                    agendamentoId,
                    statusPagamento,
                    paymentId,
                    barbeariaId, // Incluindo barbeariaId no payload
                    barbeariaUrl // Incluindo barbeariaUrl no payload
                });

                if (response.success) {
                    showToast('Agendamento e pagamento confirmados com sucesso!', 'success');
                    $('#successModal').modal('show');
                } else {
                    showToast(response.message || 'Erro ao atualizar status do pagamento.', 'danger');
                    $('#errorMessage').text(response.message || 'Erro ao atualizar status do pagamento.');
                    $('#errorModal').modal('show');
                }
            } catch (error) {
                console.error('Erro ao atualizar status do pagamento:', error);
                showToast('Erro ao atualizar o status do pagamento. Tente novamente mais tarde.', 'danger');
                $('#errorMessage').text('Erro ao atualizar o status do pagamento. Tente novamente mais tarde.');
                $('#errorModal').modal('show');
            }
        }


        function redirecionarParaMenu() {
            $('#successModal').on('hidden.bs.modal', function () {
                window.location.href = '/Cliente/MenuPrincipal';
            });
            $('#successModal').modal('show');
        }

        $('#changePaymentMethodBtn').on('click', function () {
            selectedPaymentMethod = null;
            $('#payment-form, #confirmarAgendamentoBtn, #changePaymentMethodBtn').hide();
            $('.payment-option').removeClass('selected').show();

            if (cardElement) {
                cardElement.unmount();
                cardElement = null;
                elements = null;
            }
        });

        $('#creditCardOption').on('click', function () { selectPaymentMethod('creditCard'); });
        $('#storeOption').on('click', function () { selectPaymentMethod('store'); });

        $('#submit').on('click', async function (e) {
            e.preventDefault();
            $('#loadingSpinner').fadeIn();

            const agendamentoConfirmado = await confirmarAgendamento();

            if (agendamentoConfirmado) {
                showToast('Confirmando pagamento...', 'info');
                await processarPagamento();
            } else {
                showToast("Erro ao confirmar o agendamento. Tente novamente.", 'danger');
            }

            $('#loadingSpinner').fadeOut();
        });

        $('#confirmarAgendamentoBtn').on('click', async function (e) {
            e.preventDefault();
            $('#loadingSpinner').fadeIn();

            const agendamentoConfirmado = await confirmarAgendamento();

            if (agendamentoConfirmado) {
                showToast('Agendamento confirmado com sucesso!', 'success');
                $('#successModal').modal('show');
            } else {
                $('#errorModal').modal('show');
            }

            $('#loadingSpinner').fadeOut();
        });

        $('#redirectMenuBtn').on('click', function () {
            window.location.href = '/Cliente/MenuPrincipal';
        });

        $('#errorRedirectBtn').on('click', function () {
            $('#errorModal').modal('hide');
        });
    }


    if ($('#barbeiroPage').length > 0) {
        // Função para aplicar máscara de telefone
        function aplicarMascaraTelefone(input) {
            input.on('input', function () {
                var inputValue = $(this).val().replace(/\D/g, '');
                if (inputValue.length === 11) {
                    var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                    $(this).val(phoneNumber);
                }
            });
        }

        // Aplicando máscara nos campos de telefone
        aplicarMascaraTelefone($('#adicionarTelefone'));
        aplicarMascaraTelefone($('#editarTelefone'));

        // Ação para o botão "Adicionar Barbeiro"
        $('#btnAdicionarBarbeiro').on('click', function () {
            $('#adicionarNome').val('');
            $('#adicionarEmail').val('');
            $('#adicionarTelefone').val('');
            $('#adicionarFoto').val('');
            $('#adicionarFotoPreview').attr('src', 'https://via.placeholder.com/100');
            $('#adicionarModal').modal('show');
        });

        // Pré-visualização da foto no modal de adição
        $('#adicionarFoto').on('change', function (event) {
            const fotoPreview = document.getElementById('adicionarFotoPreview');
            fotoPreview.src = URL.createObjectURL(event.target.files[0]);
        });

        // Upload da foto no modal de edição com barra de progresso
        $('#editarFoto').on('change', function (event) {
            const fotoPreview = document.getElementById('editarFotoPreview');
            fotoPreview.src = URL.createObjectURL(event.target.files[0]);

            const barbeiroId = $('#editarBarbeiroId').val();
            const file = event.target.files[0];
            if (!file) return;

            const formData = new FormData();
            formData.append('foto', file);

            const progressContainer = $('#uploadProgress');
            const progressBar = progressContainer.find('.progress-bar');
            progressContainer.removeClass('d-none');
            progressBar.css('width', '0%');

            $.ajax({
                url: `/Barbeiro/UploadFoto/${barbeiroId}`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                xhr: function () {
                    const xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener("progress", function (evt) {
                        if (evt.lengthComputable) {
                            const percentComplete = (evt.loaded / evt.total) * 100;
                            progressBar.css('width', `${percentComplete}%`);
                        }
                    }, false);
                    return xhr;
                },
                success: function (response) {
                    if (response.success) {
                        $('#editarFotoPreview').attr("src", "data:image/png;base64," + response.newFotoBase64);
                        showToast("Foto atualizada com sucesso!", "success");
                    } else {
                        showToast(response.message || "Erro ao atualizar a foto.", "danger");
                    }
                },
                error: function () {
                    showToast("Erro ao atualizar a foto.", "danger");
                },
                complete: function () {
                    setTimeout(function () {
                        progressContainer.addClass('d-none');
                    }, 500);
                }
            });
        });

        // Submissão do formulário de adição via AJAX
        $('#formAdicionarBarbeiro').on('submit', function (e) {
            e.preventDefault();
            const formData = new FormData(this);
            $.ajax({
                url: '/Barbeiro/Create',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    $('#adicionarModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao adicionar o barbeiro.", "danger");
                }
            });
        });

        // Visualizar foto em modal ao clicar na imagem
        $('.photo-preview').on('click', function () {
            const fotoUrl = $(this).data('foto');
            $('#visualizarFoto').attr('src', fotoUrl);
            $('#visualizarFotoModal').modal('show');
        });

        // Ação ao clicar no botão "Editar"
        $('.btnEditar').on('click', function () {
            var barbeiroId = $(this).data('id');
            $('#loadingSpinner').show();

            $.get(`/Barbeiro/Details/${barbeiroId}`, function (data) {
                $('#loadingSpinner').hide();
                $('#editarBarbeiroId').val(data.barbeiroId);
                $('#editarNome').val(data.nome);
                $('#editarEmail').val(data.email);
                $('#editarTelefone').val(data.telefone);
                $('#editarFotoPreview').attr('src', data.foto ? `data:image/png;base64,${data.foto}` : 'https://via.placeholder.com/100');
                $('#btnAdicionarServicoEditar').attr('data-barbeiro-id', data.barbeiroId);

                var servicosContainer = $('#servicosContainer');
                servicosContainer.empty();

                if (data.servicos && data.servicos.length > 0) {
                    data.servicos.forEach(function (servico) {
                        servicosContainer.append(`
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <div>
                                <strong>${servico.nome}</strong>
                                <span class="text-muted">R$${servico.preco.toFixed(2)} (${servico.duracao} min)</span>
                            </div>
                            <button class="btn btn-danger btn-sm btnDesvincular" data-barbeiro-id="${data.barbeiroId}" data-servico-id="${servico.servicoId}">
                                Desvincular
                            </button>
                        </div>
                    `);
                    });
                } else {
                    servicosContainer.append('<div class="text-muted">Nenhum serviço associado.</div>');
                }

                $('#editarModal').modal('show');
            }).fail(function () {
                showToast('Erro ao carregar os detalhes do barbeiro.', 'danger');
            });
        });

        $('#btnAdicionarServicoEditar').on('click', function (e) {
            e.preventDefault(); // Impede o envio do formulário
            console.log('Abrindo modal de adicionar serviço...');
            var barbeiroId = $(this).data('barbeiro-id');
            $('#servicosDisponiveisContainer').empty();

            $.get(`/Barbeiro/ObterServicosNaoVinculados`, { barbeiroId: barbeiroId }, function (data) {
                console.log('Serviços recebidos:', data);
                if (data && data.length > 0) {
                    data.forEach(function (servico) {
                        $('#servicosDisponiveisContainer').append(`
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <div>
                        <strong>${servico.nome}</strong>
                        <span class="text-muted">R$${servico.preco.toFixed(2)} (${servico.duracao} min)</span>
                    </div>
                    <button class="btn btn-primary btn-sm btnVincular" data-barbeiro-id="${barbeiroId}" data-servico-id="${servico.servicoId}">
                        Adicionar
                    </button>
                </div>
            `);
                    });
                } else {
                    $('#servicosDisponiveisContainer').append('<div class="text-muted">Nenhum serviço disponível para adicionar.</div>');
                }

                $('#adicionarServicoModal').modal('show');
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.error('Erro ao carregar serviços:', textStatus, errorThrown);
                showToast('Erro ao carregar os serviços disponíveis.', 'danger');
            });
        });


        // Ação de vincular serviço
        $(document).on('click', '.btnVincular', function () {
            var barbeiroId = $(this).data('barbeiro-id');
            var servicoId = $(this).data('servico-id');
            var button = $(this);

            button.prop('disabled', true);
            $.ajax({
                url: `/Barbeiro/VincularServico`,
                type: 'POST',
                data: { barbeiroId: barbeiroId, servicoId: servicoId },
                success: function (response) {
                    if (response.success) {
                        showToast('Serviço adicionado com sucesso!', 'success');
                        button.closest('.d-flex').remove();
                        $('#servicosContainer').append(`
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <div>
                                <strong>${response.servico.nome}</strong>
                                <span class="text-muted">R$${response.servico.preco.toFixed(2)} (${response.servico.duracao} min)</span>
                            </div>
                            <button class="btn btn-danger btn-sm btnDesvincular" data-barbeiro-id="${barbeiroId}" data-servico-id="${response.servico.servicoId}">
                                Desvincular
                            </button>
                        </div>
                    `);
                    } else {
                        showToast(response.message || 'Erro ao adicionar o serviço.', 'danger');
                    }
                },
                error: function () {
                    showToast('Erro ao processar a solicitação.', 'danger');
                }
            });
        });

        // Ação de desvincular serviço
        $(document).on('click', '.btnDesvincular', function () {
            var barbeiroId = $(this).data('barbeiro-id');
            var servicoId = $(this).data('servico-id');
            var button = $(this);

            button.prop('disabled', true);
            $.ajax({
                url: `/Barbeiro/DesvincularServico`,
                type: 'POST',
                data: { barbeiroId: barbeiroId, servicoId: servicoId },
                success: function (response) {
                    if (response.success) {
                        showToast('Serviço desvinculado com sucesso!', 'success');
                        button.closest('.d-flex').remove();
                    } else {
                        showToast(response.message || 'Erro ao desvincular o serviço.', 'danger');
                        button.prop('disabled', false);
                    }
                },
                error: function () {
                    showToast('Erro ao processar a solicitação.', 'danger');
                    button.prop('disabled', false);
                }
            });
        });

        // Submissão do formulário de edição via AJAX
        $('#formEditarBarbeiro').on('submit', function (e) {
            e.preventDefault();
            console.log('Formulário de edição submetido');
            var formData = new FormData(this);
            var barbeiroId = $('#editarBarbeiroId').val();
            $('#loadingSpinner').show();

            $.ajax({
                url: `/Barbeiro/Edit/${barbeiroId}`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    console.log('Edição bem-sucedida:', response);
                    $('#loadingSpinner').hide();
                    $('#editarModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error('Erro ao editar barbeiro:', textStatus, errorThrown);
                    $('#loadingSpinner').hide();
                    showToast("Erro ao editar o barbeiro.", "danger");
                }
            });
        });

        $('.btnExcluirBarbeiro').on('click', function () {
            var barbeiroId = $(this).data('id');
            var barbeiroNome = $(this).closest('.card').find('.card-text strong:contains("Nome:")').parent().text().replace('Nome: ', '').trim();
            $('#excluirBarbeiroNome').text(barbeiroNome);
            $('#btnConfirmarExcluir').data('id', barbeiroId);
            $('#excluirModal').modal('show');
        });

        // Confirmação de exclusão
        $('#btnConfirmarExcluir').on('click', function () {
            var barbeiroId = $(this).data('id');
            $('#loadingSpinner').show();

            $.ajax({
                url: '/Barbeiro/DeleteConfirmed',
                type: 'POST',
                data: { id: barbeiroId },
                success: function (response) {
                    $('#loadingSpinner').hide();
                    $('#excluirModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    showToast("Erro ao excluir o barbeiro.", "danger");
                }
            });
        });
    }





    // Lógica do login administrativo
    if ($('#adminLoginPageAdm').length > 0) {

        const mensagens = [
            "Bem-vindo ao ambiente administrativo da BarberShop",
            "O Ambiente administrativo perfeito para sua barbearia"
        ];
        let indiceMensagemAtual = 0;

        function atualizarMensagem() {
            $('#adminWelcomeTextAdm').fadeOut(1000, function () {
                $(this).text(mensagens[indiceMensagemAtual]).fadeIn(1000);
                indiceMensagemAtual = (indiceMensagemAtual + 1) % mensagens.length;
            });
        }

        setInterval(atualizarMensagem, 7000);

        let tempoContagemRegressivaAdm = 30;
        let intervaloContagemRegressivaAdm;

        function iniciarContagemRegressivaAdm() {
            tempoContagemRegressivaAdm = 30;
            $('#adminCountdownTimerAdm').text(tempoContagemRegressivaAdm);
            $('#resendCodeLinkAdm').hide();
            intervaloContagemRegressivaAdm = setInterval(function () {
                tempoContagemRegressivaAdm--;
                $('#adminCountdownTimerAdm').text(tempoContagemRegressivaAdm);
                if (tempoContagemRegressivaAdm <= 0) {
                    clearInterval(intervaloContagemRegressivaAdm);
                    $('#resendCodeLinkAdm').show();
                }
            }, 1000);
        }

        function validarEmailAdm(email) {
            const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return regex.test(email);
        }

        $('#togglePasswordAdm').on('click', function () {
            const campoSenha = $('#adminPasswordInputAdm');
            const icone = $(this).find('i');

            if (campoSenha.attr('type') === 'password') {
                campoSenha.attr('type', 'text');
                icone.removeClass('fa-eye').addClass('fa-eye-slash');
            } else {
                campoSenha.attr('type', 'password');
                icone.removeClass('fa-eye-slash').addClass('fa-eye');
            }
        });

        $('#adminLoginFormAdm').on('submit', function (e) {
            e.preventDefault();

            const email = $('#adminEmailInputAdm').val().trim();
            const senha = $('#adminPasswordInputAdm').val().trim();
            const barbeariaUrl = $('#adminLoginFormAdm input[name="barbeariaUrl"]').val(); // Obtendo a URL da barbearia

            if (!validarEmailAdm(email)) {
                showToast('Por favor, insira um e-mail válido.', 'danger');
                return;
            }

            if (senha.length === 0) {
                showToast('Por favor, insira sua senha.', 'danger');
                return;
            }

            $('#adminFullScreenSpinnerAdm').fadeIn(); // Mostra o spinner
            $('#adminSubmitButtonAdm').prop('disabled', true);

            const formData = $(this).serialize();
            $.ajax({
                type: 'POST',
                url: `/${barbeariaUrl}/Login/AdminLogin`, // URL com a barbeariaUrl
                data: formData,
                success: function (data) {
                    $('#adminFullScreenSpinnerAdm').fadeOut(); // Esconde o spinner após sucesso
                    if (data.success) {
                        $('#usuarioIdFieldAdm').val(data.usuarioId);
                        $('#adminSubmitButtonAdm').prop('disabled', false);
                        $('#verificationModalAdm').modal('show');
                        iniciarContagemRegressivaAdm();
                    } else {
                        $('#adminSubmitButtonAdm').prop('disabled', false);
                        showToast(data.message, 'danger');
                    }
                },
                error: function (xhr, status, error) {
                    $('#adminFullScreenSpinnerAdm').fadeOut(); // Esconde o spinner após erro
                    $('#adminSubmitButtonAdm').prop('disabled', false);
                    showToast('Ocorreu um erro. Por favor, tente novamente.', 'danger');
                }
            });
        });

        $('#resendCodeAdm').on('click', function (e) {
            e.preventDefault();

            const usuarioId = $('#usuarioIdFieldAdm').val();
            const barbeariaUrl = $('#adminLoginFormAdm input[name="barbeariaUrl"]').val(); // Obtendo a URL da barbearia

            $.ajax({
                type: 'GET',
                url: `/${barbeariaUrl}/Login/ReenviarCodigoAdm?usuarioId=${usuarioId}`,
                success: function (data) {
                    if (data.success) {
                        showToast("Código de verificação reenviado!", 'info');
                        iniciarContagemRegressivaAdm();
                    } else {
                        showToast(data.message, 'danger');
                    }
                },
                error: function (xhr, status, error) {
                    showToast('Erro ao reenviar o código. Tente novamente.', 'danger');
                }
            });
        });

        $('#verificationFormAdm').on('submit', function (e) {
            e.preventDefault();

            $('#verifySpinnerAdm').show();
            $('#VerifyCodeAdm').prop('disabled', true);

            const dadosVerificacao = $(this).serialize();
            const barbeariaUrl = $('#adminLoginFormAdm input[name="barbeariaUrl"]').val(); // Obtendo a URL da barbearia

            $.ajax({
                type: 'POST',
                url: `/${barbeariaUrl}/Login/VerificarAdminCodigo`, // URL com a barbeariaUrl
                data: dadosVerificacao,
                success: function (data) {
                    $('#verifySpinnerAdm').hide();
                    $('#VerifyCodeAdm').prop('disabled', false);

                    if (data.success) {
                        window.location.href = data.redirectUrl;
                    } else {
                        $('#codeErrorMessageAdm').show();
                        showToast(data.message, 'danger');
                    }
                },
                error: function (xhr, status, error) {
                    $('#verifySpinnerAdm').hide();
                    $('#VerifyCodeAdm').prop('disabled', false);
                    showToast('Erro ao verificar o código. Tente novamente.', 'danger');
                }
            });
        });

        // Redefinir senha admin
        $('#forgotPasswordLinkAdm').on('click', function () {
            $('#forgotPasswordModalAdm').modal('show');
        });

        $('#forgotPasswordFormAdm').on('submit', function (event) {
            event.preventDefault();

            const email = $('#forgotPasswordEmailAdm').val().trim();
            const barbeariaUrl = $('#adminLoginFormAdm input[name="barbeariaUrl"]').val();

            if (!validarEmailAdm(email)) {
                showToast('Por favor, insira um e-mail válido.', 'danger');
                return;
            }

            showToast("Solicitando redefinição de senha para: " + email, 'info');
            $.ajax({
                url: `/${barbeariaUrl}/Login/SolicitarRecuperacaoSenhaAdmin`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(email),
                success: function (data) {
                    if (data.success) {
                        $('#forgotPasswordModalAdm').modal('hide');
                        showToast("Instruções de recuperação de senha enviadas para o seu e-mail.", 'success');
                    } else {
                        showToast(data.message || "Erro ao solicitar recuperação de senha.", 'danger');
                    }
                },
                error: function (xhr, status, error) {
                    showToast("Erro ao processar a solicitação de redefinição de senha.", 'danger');
                }
            });
        });
    }



    // Verifica se o elemento com o ID 'adminDashboard' está presente na página
    if ($('#adminDashboard').length > 0) {


        function adjustDashboardView() {
            const mobileMessage = document.getElementById("mobileMessage");
            const buttonContainer = document.getElementById("buttonContainer");
            const dashboardContainer = document.getElementById("dashboard-container");

            // Verifica se a largura da janela é menor ou igual a 768px (modo mobile)
            if (window.innerWidth <= 768) {
                mobileMessage.classList.remove("d-none"); // Mostra a mensagem
                buttonContainer.style.display = "none"; // Esconde o botão
                dashboardContainer.style.display = "none"; // Esconde os gráficos
            } else {
                mobileMessage.classList.add("d-none"); // Esconde a mensagem
                buttonContainer.style.display = "flex"; // Mostra o botão
                dashboardContainer.style.display = "flex"; // Mostra os gráficos
            }
        }

        // Detecta alterações no tamanho da janela e ajusta a interface
        window.addEventListener("resize", adjustDashboardView);

        // Ajusta a interface ao carregar a página
        document.addEventListener("DOMContentLoaded", adjustDashboardView);

        // Função para buscar os dados do backend
        async function fetchDashboardData() {
            try {
                const response = await $.ajax({
                    url: "/Dashboard/GetDashboardData",
                    method: "GET",
                });
                return response;
            } catch (error) {
                console.error("Erro ao obter dados do dashboard:", error);
                throw error;
            }
        }

        // Configuração padrão das posições dos gráficos
        const defaultPositions = [
            { GraficoId: "lucroSemanaChart", Posicao: 0 },
            { GraficoId: "lucroMesChart", Posicao: 1 },
            { GraficoId: "agendamentosSemanaChart", Posicao: 2 },
            { GraficoId: "servicosMaisSolicitadosChart", Posicao: 3 },
            { GraficoId: "lucroPorBarbeiroChart", Posicao: 4 },
            { GraficoId: "atendimentosPorBarbeiroChart", Posicao: 5 }
        ];

        // Função para inicializar o dashboard
        async function initializeDashboard() {
            $('#dashboard-container').empty();
            try {
                const data = await fetchDashboardData();
                if (data && Object.values(data).some(item => item && ((Array.isArray(item) && item.length > 0) || (typeof item === 'object' && Object.keys(item).length > 0)))) {
                    window.dashboardData = data;
                    initializeDashboardCharts(data); // Inicializa os gráficos com os dados recebidos
                    await loadCustomReportsFromLocalStorage(); // Carrega relatórios personalizados salvos no localStorage
                } else {
                    displayDefaultLayout(); // Aplica layout padrão se os dados estiverem vazios
                }
            } catch (error) {
                console.error("Erro ao inicializar o dashboard:", error);
                displayDefaultLayout();
            }
        }

        // Função para exibir um layout padrão se não houver dados
        function displayDefaultLayout() {
            $('#dashboard-container').html(`
            <div class="text-center my-5">
                <h4>Nenhum dado disponível</h4>
                <p>Adicione alguns registros para visualizar os gráficos.</p>
            </div>
        `);
        }

        // Função para renderizar o gráfico com a configuração fornecida e adicionar o menu de exportação
        function renderChart(config, chartId) {
            try {
                $('#dashboard-container').append(`
                <div class="col-lg-4 col-md-6 col-12 dashboard-card" id="${chartId}Card">
                    <div class="card">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h5>${config.title || "Gráfico"}</h5>
                            <div class="dropdown">
                                <button class="btn btn-light btn-sm" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="bi bi-three-dots"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li><a class="dropdown-item export-to-excel" href="#" data-chart="${chartId}">Exportar para Excel</a></li>
                                </ul>
                            </div>
                        </div>
                        <div class="card-body">
                            <canvas id="${chartId}Canvas"></canvas>
                        </div>
                    </div>
                </div>
            `);
                initializeChart(`${chartId}Canvas`, config.config); // Usar um ID único para o canvas
            } catch (error) {
                console.error(`Erro ao renderizar o gráfico ${chartId}:`, error);
            }
        }

        // Função para inicializar cada gráfico
        function initializeChart(ctxId, config) {
            const ctx = document.getElementById(ctxId);
            if (ctx) {
                const existingChart = Chart.getChart(ctxId);
                if (existingChart) existingChart.destroy();
                try {
                    new Chart(ctx.getContext('2d'), config);
                } catch (error) {
                    console.error(`Erro ao inicializar o gráfico ${ctxId}:`, error);
                }
            } else {
                console.warn(`Canvas com ID ${ctxId} não encontrado.`);
            }
        }

        // Função para exportar o gráfico para Excel com dados adicionais
        function exportChartToExcel(chartId) {
            const chart = Chart.getChart(`${chartId}Canvas`);
            if (!chart) {
                console.error("Gráfico não encontrado:", chartId);
                return;
            }

            // Dados básicos do gráfico
            const labels = chart.data.labels;
            const dataSet = chart.data.datasets[0];

            // Adicionando dados adicionais ao Excel
            const additionalData = dataSet.data.map((value, index) => ({
                Label: labels[index],
                Valor: value,
                Percentual: ((value / dataSet.data.reduce((a, b) => a + b, 0)) * 100).toFixed(2) + '%', // Percentual do total
                Cor: dataSet.backgroundColor ? dataSet.backgroundColor[index] : "N/A" // Cor do item (se disponível)
            }));

            // Converte os dados para uma planilha Excel
            const worksheet = XLSX.utils.json_to_sheet(additionalData);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "Dados do Gráfico");

            // Inicia o download do arquivo Excel
            XLSX.writeFile(workbook, `${chartId}.xlsx`);
        }


        // Evento para exportar o gráfico ao clicar no item do menu
        $(document).on("click", ".export-to-excel", function (e) {
            e.preventDefault();
            const chartId = $(this).data("chart");
            exportChartToExcel(chartId);
        });

        // Função para obter a configuração do gráfico pelo ID
        function getChartConfigById(id) {
            const data = window.dashboardData;
            const chartConfigs = {
                lucroSemanaChart: createChartConfig('line', data.lucroDaSemana, 'Lucro da Semana', ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom']),
                lucroMesChart: createChartConfig('line', data.lucroDoMes, 'Lucro do Mês', ['Semana 1', 'Semana 2']),
                agendamentosSemanaChart: createChartConfig('bar', data.agendamentosPorSemana, 'Agendamentos', ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom']),
                servicosMaisSolicitadosChart: createChartConfig('pie', Object.values(data.servicosMaisSolicitados), 'Serviços Mais Solicitados', Object.keys(data.servicosMaisSolicitados)),
                lucroPorBarbeiroChart: createChartConfig('bar', Object.values(data.lucroPorBarbeiro), 'Lucro por Barbeiro', Object.keys(data.lucroPorBarbeiro)),
                atendimentosPorBarbeiroChart: createChartConfig('doughnut', Object.values(data.atendimentosPorBarbeiro), 'Atendimentos por Barbeiro', Object.keys(data.atendimentosPorBarbeiro))
            };

            return chartConfigs[id] || null;
        }

        // Função para criar configurações de gráficos
        function createChartConfig(type, data, label, labels) {
            return {
                type: type,
                data: {
                    labels: labels,
                    datasets: [{
                        label: label,
                        data: data,
                        backgroundColor: generateBackgroundColors(data.length),
                        borderColor: generateBorderColors(data.length),
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: true, labels: { font: { size: 14 } } },
                        tooltip: { callbacks: { label: context => `${label}: ${context.raw}` } }
                    },
                    scales: type !== 'pie' && type !== 'doughnut' ? {
                        y: { beginAtZero: true, title: { display: true, text: label } },
                        x: { ticks: { font: { size: 12 } } }
                    } : {}
                }
            };
        }

        // Função para adicionar relatório personalizado e salvar a chave no localStorage
        async function addCustomReport() {
            const reportType = $('#reportType').val();
            const periodDays = $('#reportPeriod').val();
            const reportKey = `${reportType}-${periodDays}`; // Chave única para cada relatório personalizado

            try {
                const data = await $.ajax({
                    url: `/Dashboard/GetCustomReportData?reportType=${reportType}&periodDays=${periodDays}`,
                    method: "GET"
                });


                const chartConfig = {
                    title: getReportTitle(reportType),
                    config: createChartConfig('bar', Object.values(data), getReportTitle(reportType), Object.keys(data))
                };

                const chartId = `${reportType}CustomChart${Date.now()}`;
                renderChart(chartConfig, chartId);

                // Salva a chave do relatório no localStorage
                saveReportKeyToLocalStorage(reportKey);

                $('#addReportModal').modal('hide');
            } catch (error) {
                console.error("Erro ao adicionar relatório personalizado:", error);
                alert("Erro ao adicionar relatório. Por favor, tente novamente.");
            }
        }

        // Função para salvar uma chave de relatório no localStorage
        function saveReportKeyToLocalStorage(reportKey) {
            let reportKeys = JSON.parse(localStorage.getItem('customReports')) || [];
            if (!reportKeys.includes(reportKey)) {
                reportKeys.push(reportKey);
                localStorage.setItem('customReports', JSON.stringify(reportKeys));
            }
        }

        // Função para carregar relatórios personalizados do localStorage ao iniciar o dashboard
        async function loadCustomReportsFromLocalStorage() {
            const reportKeys = JSON.parse(localStorage.getItem('customReports')) || [];
            for (const reportKey of reportKeys) {
                const [reportType, periodDays] = reportKey.split('-');
                try {
                    const data = await $.ajax({
                        url: `/Dashboard/GetCustomReportData?reportType=${reportType}&periodDays=${periodDays}`,
                        method: "GET"
                    });

                    const chartConfig = {
                        title: getReportTitle(reportType),
                        config: createChartConfig('bar', Object.values(data), getReportTitle(reportType), Object.keys(data))
                    };

                    const chartId = `${reportType}CustomChart${Date.now()}`;
                    renderChart(chartConfig, chartId);
                } catch (error) {
                    console.error(`Erro ao carregar relatório ${reportType}:`, error);
                }
            }
        }

        // Função auxiliar para obter o título do relatório
        function getReportTitle(reportType) {
            const titles = {
                "agendamentosPorStatus": "Agendamentos por Status",
                "servicosMaisSolicitados": "Serviços Mais Solicitados",
                "lucroPorFormaPagamento": "Lucro por Forma de Pagamento",
                "atendimentosPorBarbeiro": "Atendimentos por Barbeiro",
                "clientesFrequentes": "Clientes Frequentes",
                "pagamentosPorStatus": "Pagamentos por Status",
                "servicosPorPreco": "Serviços por Faixa de Preço",
                "lucroPorPeriodo": "Lucro Diário/Semanal/Mensal",
                "tempoMedioPorServico": "Tempo Médio por Tipo de Serviço",
                "agendamentosCancelados": "Agendamentos Cancelados"
            };
            return titles[reportType] || "Relatório Personalizado";
        }

        // Função auxiliar para gerar cores de fundo
        function generateBackgroundColors(count) {
            const colors = ['rgba(255, 99, 132, 0.5)', 'rgba(54, 162, 235, 0.5)', 'rgba(255, 206, 86, 0.5)', 'rgba(75, 192, 192, 0.5)'];
            return Array.from({ length: count }, (_, i) => colors[i % colors.length]);
        }

        // Função auxiliar para gerar cores de borda
        function generateBorderColors(count) {
            const colors = ['rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)', 'rgba(75, 192, 192, 1)'];
            return Array.from({ length: count }, (_, i) => colors[i % colors.length]);
        }

        // Função para inicializar todos os gráficos no dashboard
        function initializeDashboardCharts(data) {
            window.dashboardData = data; // Armazena os dados globalmente para uso em restoreInitialPositions
            const chartConfigs = [
                { id: 'lucroSemanaChart', data: data.lucroDaSemana, type: 'line', title: 'Lucro da Semana', labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'] },
                { id: 'lucroMesChart', data: data.lucroDoMes, type: 'line', title: 'Lucro do Mês', labels: ['Semana 1', 'Semana 2'] },
                { id: 'agendamentosSemanaChart', data: data.agendamentosPorSemana, type: 'bar', title: 'Agendamentos', labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'] },
                { id: 'servicosMaisSolicitadosChart', data: Object.values(data.servicosMaisSolicitados), type: 'pie', title: 'Serviços Mais Solicitados', labels: Object.keys(data.servicosMaisSolicitados) },
                { id: 'lucroPorBarbeiroChart', data: Object.values(data.lucroPorBarbeiro), type: 'bar', title: 'Lucro por Barbeiro', labels: Object.keys(data.lucroPorBarbeiro) },
                { id: 'atendimentosPorBarbeiroChart', data: Object.values(data.atendimentosPorBarbeiro), type: 'doughnut', title: 'Atendimentos por Barbeiro', labels: Object.keys(data.atendimentosPorBarbeiro) },
            ];

            chartConfigs.forEach(config => {
                if (config.data && config.data.length > 0) {
                    renderChart({ title: config.title, config: createChartConfig(config.type, config.data, config.title, config.labels) }, config.id);
                }
            });
        }

        // Inicialização do Dashboard ao carregar o documento
        $(document).ready(function () {
            $('#addReportButton').on('click', addCustomReport);
            initializeDashboard();
        });
    }



    if ($('#servicoPage').length > 0) {
        function aplicarMascaraPreco(input) {
            input.on('input', function () {
                let valor = $(this).val().replace(/\D/g, '');
                valor = (valor / 100).toFixed(2) + '';
                valor = valor.replace(".", ",");
                $(this).val(valor);
            });
        }

        aplicarMascaraPreco($('#adicionarPreco'));
        aplicarMascaraPreco($('#editarPreco'));

        function converterPrecoParaFloat(precoFormatado) {
            const valorFloat = parseFloat(precoFormatado.replace(/\./g, '').replace(',', '.'));
            return valorFloat;
        }

        function mostrarLoading() {
            $('#loadingSpinnerServico').show();
        }

        function ocultarLoading() {
            $('#loadingSpinnerServico').hide();
        }

        $('#btnAdicionarServico').on('click', function () {
            $('#adicionarNome').val('');
            $('#adicionarPreco').val('');
            $('#adicionarDuracao').val('');
            $('#adicionarModal').modal('show');
        });

        $('#formAdicionarServico').on('submit', function (e) {
            e.preventDefault();
            mostrarLoading();

            const precoFormatado = $('#adicionarPreco').val();
            const precoFloat = converterPrecoParaFloat(precoFormatado);

            const formData = {
                Nome: $('#adicionarNome').val(),
                Preco: precoFloat,
                Duracao: $('#adicionarDuracao').val()
            };

            $.ajax({
                url: '/Servico/Create',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                success: function (response) {
                    $('#adicionarModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500);
                    }
                },
                error: function () {
                    showToast('Erro ao adicionar o serviço.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        $(document).on('click', '.btnEditar', function () {
            const servicoId = $(this).data('id');
            mostrarLoading();

            $.get(`/Servico/Details/${servicoId}`, function (data) {
                $('#editarServicoId').val(data.servicoId);
                $('#editarNome').val(data.nome);
                $('#editarPreco').val(data.preco.toFixed(2).replace('.', ','));
                $('#editarDuracao').val(data.duracao);
                $('#editarModal').modal('show');
            }).always(function () {
                ocultarLoading();
            });
        });

        $('#formEditarServico').on('submit', function (e) {
            e.preventDefault();
            mostrarLoading();

            const precoFormatado = $('#editarPreco').val();
            const precoFloat = converterPrecoParaFloat(precoFormatado);

            const formData = {
                ServicoId: $('#editarServicoId').val(),
                Nome: $('#editarNome').val(),
                Preco: precoFloat,
                Duracao: $('#editarDuracao').val()
            };

            $.ajax({
                url: `/Servico/Edit/${formData.ServicoId}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                success: function (response) {
                    $('#editarModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500);
                    }
                },
                error: function () {
                    showToast('Erro ao editar o serviço.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        $(document).on('click', '.btnExcluir', function () {
            const servicoId = $(this).data('id');
            const servicoNome = $(this).closest('tr').find('td:first').text();
            $('#excluirServicoNome').text(servicoNome);
            $('#btnConfirmarExcluir').data('id', servicoId);
            $('#excluirModal').modal('show');
        });

        $('#btnConfirmarExcluir').on('click', function () {
            const servicoId = $(this).data('id');
            mostrarLoading();

            $.ajax({
                url: '/Servico/DeleteConfirmed',
                type: 'POST',
                data: { id: servicoId },
                success: function (response) {
                    $('#excluirModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500);
                    }
                },
                error: function () {
                    showToast('Erro ao excluir o serviço.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });
    }




    if ($('#pagamentoPage').length > 0) {

        function mostrarLoading() {
            $('#loadingSpinnerPagamento').show();
        }

        function ocultarLoading() {
            $('#loadingSpinnerPagamento').hide();
        }

        // Ação para buscar agendamentos e exibir no modal de Inserir Pagamento
        $('#buscarAgendamentosBtn').on('click', function () {
            const dataInicio = $('#dataInicio').val();
            const dataFim = $('#dataFim').val();

            if (!dataInicio) {
                alert('Por favor, selecione uma data de início.');
                return;
            }

            mostrarLoading();

            $.ajax({
                url: '/Agendamento/ObterAgendamentosPorData', // Endpoint para buscar agendamentos com serviços e valores
                type: 'GET',
                data: { dataInicio: dataInicio, dataFim: dataFim },
                success: function (agendamentos) {
                    $('#agendamentosContainer').html('');
                    if (agendamentos.length > 0) {
                        agendamentos.forEach(agendamento => {
                            let servicosHTML = '';
                            let valorTotal = agendamento.precoTotal;

                            // Adiciona cada serviço ao HTML e calcula o valor total
                            agendamento.servicos.forEach(servico => {
                                servicosHTML += `<li>${servico.nome} - R$ ${servico.preco.toFixed(2)}</li>`;
                            });

                            $('#agendamentosContainer').append(`
                            <div class="d-flex flex-column border p-3 mb-3">
                                <p><strong>Cliente:</strong> ${agendamento.cliente.nome}</p>
                                <p><strong>Barbeiro:</strong> ${agendamento.barbeiroNome}</p>
                                <p><strong>Horário:</strong> ${new Date(agendamento.dataHora).toLocaleTimeString()}</p>
                                <p><strong>Serviços:</strong></p>
                                <ul>${servicosHTML}</ul>
                                <p><strong>Valor Total:</strong> R$ ${valorTotal.toFixed(2)}</p>
                                <button class="btn btn-success btnInserirPagamento" data-agendamento-id="${agendamento.agendamentoId}" data-valor-total="${valorTotal.toFixed(2)}">
                                    Inserir Pagamento
                                </button>
                            </div>
                        `);
                        });

                        // Ação para inserir pagamento manualmente
                        $('.btnInserirPagamento').on('click', function () {
                            const agendamentoId = $(this).data('agendamento-id');
                            const valorTotal = $(this).data('valor-total');

                            mostrarLoading();
                            $.ajax({
                                url: '/Pagamento/Inserir',  // Endpoint para inserir pagamento
                                type: 'POST',
                                data: JSON.stringify({ agendamentoId: agendamentoId, valorPago: valorTotal }),
                                contentType: 'application/json',
                                success: function (response) {
                                    alert(response.message);
                                    if (response.success) {
                                        location.reload();
                                    }
                                },
                                error: function () {
                                    alert('Erro ao inserir pagamento.');
                                },
                                complete: function () {
                                    ocultarLoading();
                                }
                            });
                        });
                    } else {
                        $('#agendamentosContainer').html('<p>Nenhum agendamento encontrado para esta data.</p>');
                    }
                },
                error: function () {
                    alert('Erro ao buscar agendamentos.');
                    console.error("Erro ao buscar agendamentos.");
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        // Ação para o botão "Ver Detalhes"
        $('.btnDetalhes').on('click', function () {
            const pagamentoId = $(this).data('id');
            mostrarLoading();

            $.get(`/Pagamento/Detalhes/${pagamentoId}`, function (data) {
                const valorPago = data.valorPago ? parseFloat(data.valorPago).toFixed(2).replace('.', ',') : 'N/A';
                $('#detalhesModalBody').html(`
                <p><strong>Cliente:</strong> ${data.nomeCliente}</p>
                <p><strong>Valor Pago:</strong> R$ ${valorPago}</p>
                <p><strong>Status:</strong> ${data.statusPagamento}</p>
                <p><strong>Data do Pagamento:</strong> ${data.dataPagamento}</p>
            `);
                $('#detalhesModal').modal('show');
            }).fail(function () {
                showToast('Erro ao carregar os detalhes do pagamento.', 'danger');
            }).always(function () {
                ocultarLoading();
            });
        });

        // Ação para o botão de solicitar reembolso
        $('.btnReembolso').on('click', function () {
            const pagamentoId = $(this).data('id');
            const paymentId = $(this).data('payment-id');
            const nomeCliente = $(this).data('nome');
            const valorPago = $(this).data('valor');

            $('#reembolsoPagamentoId').text(`${nomeCliente} no valor de R$ ${valorPago}`);
            $('#hiddenReembolsoPaymentId').val(paymentId);
            $('#btnConfirmarReembolso').data('id', pagamentoId);
            $('#reembolsoModal').modal('show');
        });

        // Confirmação de reembolso via AJAX
        $('#btnConfirmarReembolso').on('click', function () {
            const pagamentoId = $(this).data('id');
            const paymentId = $('#hiddenReembolsoPaymentId').val();
            mostrarLoading();

            $.ajax({
                url: '/api/payment/refund',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ PaymentId: paymentId }),
                success: function (response) {
                    $('#reembolsoModal').modal('hide');
                    showToast('Reembolso processado com sucesso.', 'success');
                    $.ajax({
                        url: `/Pagamento/SolicitarReembolso/${pagamentoId}`,
                        type: 'POST',
                        success: function () {
                            setTimeout(() => location.reload(), 1500);
                        }
                    });
                },
                error: function () {
                    showToast('Erro ao solicitar reembolso.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        // Ação para o botão de exclusão
        $('.btnExcluir').on('click', function () {
            const pagamentoId = $(this).data('id');
            $('#excluirPagamentoNome').text(pagamentoId);
            $('#btnConfirmarExcluir').data('id', pagamentoId);
            $('#excluirModal').modal('show');
        });
    }




    if ($('#avaliacaoPage').length > 0) {
        let notaServico = 0;
        let notaBarbeiro = 0;

        // Inicialmente, desabilita o botão de enviar
        $('#avaliacaoEnviarBtn').prop('disabled', true);

        // Função para exibir o spinner no botão
        function mostrarSpinnerBotao() {
            $('#avaliacaoEnviarBtn').html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Enviando...
        `).prop('disabled', true);
        }

        // Função para verificar se o botão deve ser habilitado
        function atualizarEstadoBotao() {
            if (notaServico > 0 && notaBarbeiro > 0) {
                $('#avaliacaoEnviarBtn').prop('disabled', false);
            } else {
                $('#avaliacaoEnviarBtn').prop('disabled', true);
            }
        }

        // Ação para marcar estrelas ao passar o mouse
        $('.avaliacao-estrela').on('mouseenter', function () {
            $(this).prevAll().addBack().addClass('hover');
        }).on('mouseleave', function () {
            $('.avaliacao-estrela').removeClass('hover');
        });

        // Ação para clicar nas estrelas
        $('.avaliacao-estrela').on('click', function () {
            const valor = $(this).data('value');
            const parentId = $(this).parent().attr('id'); // Identifica o pai das estrelas

            if (parentId === 'avaliacaoServicos') {
                notaServico = valor;
                $('#avaliacaoServicos .avaliacao-estrela').removeClass('selecionada');
                $(this).prevAll().addBack().addClass('selecionada');
            } else if (parentId === 'avaliacaoBarbeiro') {
                notaBarbeiro = valor;
                $('#avaliacaoBarbeiro .avaliacao-estrela').removeClass('selecionada');
                $(this).prevAll().addBack().addClass('selecionada');
            }

            // Exibir campo de observação caso as duas avaliações estejam preenchidas
            if (notaServico > 0 && notaBarbeiro > 0) {
                $('#avaliacaoObservacaoContainer')
                    .addClass('visible')
                    .css({ opacity: 1, 'max-height': '150px' });
            }

            // Atualiza o estado do botão
            atualizarEstadoBotao();
        });

        // Ação para enviar a avaliação
        $('#avaliacaoEnviarBtn').on('click', function () {
            const observacao = $('#avaliacaoObservacao').val();
            const agendamentoId = $('#agendamentoId').val(); // Obtém o ID do input hidden

            if (notaServico > 0 && notaBarbeiro > 0) {
                mostrarSpinnerBotao();

                // Envio real para o backend via AJAX
                $.ajax({
                    url: '/Avaliacao/Create',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        AgendamentoId: agendamentoId,
                        NotaServico: notaServico,
                        NotaBarbeiro: notaBarbeiro,
                        Observacao: observacao
                    }),
                    success: function (response) {
                        if (response.success) {
                            // Ocultar o formulário e exibir a mensagem de agradecimento
                            $('.avaliacao-container').fadeOut(300, function () {
                                $('#avaliacaoMensagemAgradecimento')
                                    .fadeIn(300)
                                    .css('display', 'block');
                            });
                        } else {
                            // Exibir mensagem de erro
                            showToast(response.message || 'Erro ao enviar a avaliação.', 'danger');
                            $('#avaliacaoEnviarBtn')
                                .html('Enviar Avaliação')
                                .prop('disabled', false);
                        }
                    },
                    error: function () {
                        // Exibir mensagem de erro em caso de falha
                        showToast(
                            'Ocorreu um erro ao enviar sua avaliação. Tente novamente mais tarde.',
                            'danger'
                        );
                        $('#avaliacaoEnviarBtn')
                            .html('Enviar Avaliação')
                            .prop('disabled', false);
                    }
                });
            } else {
                showToast(
                    'Por favor, selecione as avaliações para o serviço e o barbeiro.',
                    'warning'
                );
            }
        });
    }





    const pageErro = document.getElementById('erro-barbearia-container');

    if (pageErro) {
        const piadas = [
            "Parece que esta barbearia fez a barba... e sumiu!",
            "Essa barbearia deve estar se escondendo atrás do espelho.",
            "Ops! Essa barbearia foi dar um 'tapa no visual' e desapareceu.",
            "Acho que essa barbearia foi fazer uma escova progressiva... para bem longe!",
            "Talvez a barbearia esteja ocupada fazendo a barba do Pé Grande.",
            "Essa barbearia foi cortar os laços com a internet!",
            "Parece que a barbearia saiu para comprar mais gel fixador e se perdeu no caminho.",
            "Ela foi tão longe no estilo que virou lenda urbana!",
            "Será que a barbearia foi chamada para um serviço secreto de corte de cabelo?"
        ];

        const mensagemContainer = document.getElementById("erro-barbearia-mensagem");
        let piadaIndex = 0;

        function alternarMensagem() {
            mensagemContainer.style.opacity = 0; // Esconde a mensagem atual
            setTimeout(() => {
                if (piadaIndex % 2 === 0) {
                    mensagemContainer.textContent = "Ops! Essa barbearia foi tão bem barbeada que desapareceu!";
                } else {
                    mensagemContainer.textContent = piadas[Math.floor(piadaIndex / 2)];
                }
                mensagemContainer.style.opacity = 1; // Mostra a nova mensagem
                piadaIndex = (piadaIndex + 1) % (piadas.length * 2); // Alterna entre mensagem e piada
            }, 500); // Tempo de transição entre mensagens
        }

        alternarMensagem(); // Exibe a primeira mensagem imediatamente
        setInterval(alternarMensagem, 5000); // Troca de mensagem a cada 5 segundos
    }
    var agendamentosPage = document.getElementById('agendamentoPage');

    if (agendamentosPage) {

        // Função para aplicar máscara de valor
        function aplicarMascaraPreco(input) {
            input.on('input', function () {
                let valor = $(this).val().replace(/\D/g, '');
                valor = (valor / 100).toFixed(2) + '';
                valor = valor.replace(".", ",");
                $(this).val(valor);
            });
        }

        // Aplica a máscara de preço ao campo de edição
        aplicarMascaraPreco($('#editarPrecoTotal'));

        // Converte o preço formatado (string) para float
        function converterPrecoParaFloat(precoFormatado) {
            const valorFloat = parseFloat(precoFormatado.replace(/\./g, '').replace(',', '.'));
            return valorFloat;
        }

        function mostrarLoading() {
            $('#loadingSpinnerAgendamento').show();
        }

        function ocultarLoading() {
            $('#loadingSpinnerAgendamento').hide();
        }

        // Abrir modal de edição
        $(document).on('click', '.btnEditar', function () {
            const agendamentoId = $(this).data('id');
            mostrarLoading();

            $.get(`/Agendamento/Details/${agendamentoId}`, function (data) {
                console.log("Dados do agendamento recebidos:", data);

                $('#editarAgendamentoId').val(data.AgendamentoId);

                // Ajustar DataHora para evitar alteração de fuso horário
                if (data.DataHora) {
                    const dataHora = new Date(data.DataHora);
                    console.log("DataHora convertida (sem fuso horário):", dataHora);

                    const dataLocal = `${dataHora.getFullYear()}-${(dataHora.getMonth() + 1).toString().padStart(2, '0')}-${dataHora.getDate().toString().padStart(2, '0')}`;
                    const horaLocal = `${dataHora.getHours().toString().padStart(2, '0')}:${dataHora.getMinutes().toString().padStart(2, '0')}`;

                    $('#editarDataHora').val(`${dataLocal}T${horaLocal}`);
                } else {
                    console.warn("Campo DataHora está nulo ou indefinido");
                    $('#editarDataHora').val(''); // Limpa o campo ou insere um valor padrão
                }

                $('#editarStatus').val(data.Status);
                $('#editarStatusPagamento').val(data.Pagamento ? data.Pagamento.StatusPagamento : -1);

                // Verifica se PrecoTotal existe e é um número
                if (data.PrecoTotal !== undefined && data.PrecoTotal !== null) {
                    const precoFormatado = data.PrecoTotal.toFixed(2).replace('.', ',');
                    $('#editarPrecoTotal').val(precoFormatado);
                } else {
                    console.warn("Campo PrecoTotal está nulo ou indefinido");
                    $('#editarPrecoTotal').val('');
                }

                $('#editarAgendamentoModal').modal('show');
            }).always(function () {
                ocultarLoading();
            });
        });

        // Submeter formulário de edição
        $('#formEditarAgendamento').on('submit', function (e) {
            e.preventDefault(); // Impede o comportamento padrão do formulário
            mostrarLoading();

            // Formatar o preço corretamente
            const precoFormatado = $('#editarPrecoTotal').val();
            const precoFloat = converterPrecoParaFloat(precoFormatado);

            // Criar o objeto de dados do formulário
            const formData = {
                AgendamentoId: parseInt($('#editarAgendamentoId').val()), // Garantir que seja inteiro
                DataHora: $('#editarDataHora').val(), // Formato: YYYY-MM-DDTHH:mm
                Status: parseInt($('#editarStatus').val()), // Garantir que seja inteiro
                PrecoTotal: precoFloat, // Float para decimal no C#
                Pagamento: { // Incluindo o pagamento como parte do objeto
                    StatusPagamento: parseInt($('#editarStatusPagamento').val()), // Garantir que seja inteiro
                    ValorPago: precoFloat // Passar o PrecoTotal como ValorPago
                }
            };


            console.log("Enviando dados de edição:", formData); // Log para depuração

            // Enviar os dados para o servidor via AJAX
            $.ajax({
                url: `/Agendamento/Edit/${formData.AgendamentoId}`, // O ID do agendamento na URL
                type: 'POST',
                contentType: 'application/json', // O corpo será JSON
                data: JSON.stringify(formData), // Serializar os dados para JSON
                success: function (response) {
                    console.log("Resposta da edição:", response); // Log da resposta
                    $('#editarAgendamentoModal').modal('hide'); // Fecha o modal de edição
                    showToast('Agendamento editado com sucesso!', 'success'); // Exibe o toast de sucesso
                    setTimeout(() => location.reload(), 1500); // Recarregar a página após 1,5s
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Erro na edição:", textStatus, errorThrown); // Log do erro
                    showToast('Erro ao editar o agendamento.', 'danger'); // Exibe toast de erro
                },
                complete: function () {
                    ocultarLoading(); // Oculta o loading no final
                }
            });
        });

        // Abrir modal de exclusão
        $(document).on('click', '.btnExcluir', function () {
            const agendamentoId = $(this).data('id');
            const clienteNome = $(this).closest('tr').find('td:first').text();
            console.log("Abrindo modal de exclusão para o agendamento ID:", agendamentoId, "Cliente:", clienteNome); // Log do ID do agendamento e nome do cliente

            $('#excluirAgendamentoNome').text(clienteNome);
            $('#btnConfirmarExcluir').data('id', agendamentoId);
            $('#excluirModal').modal('show');
        });

        // Confirmar exclusão
        $('#btnConfirmarExcluir').on('click', function () {
            const agendamentoId = $(this).data('id');
            console.log("Confirmando exclusão para o agendamento ID:", agendamentoId); // Log do ID do agendamento para exclusão
            mostrarLoading();

            $.ajax({
                url: '/Agendamento/DeleteConfirmed',
                type: 'POST',
                data: { id: agendamentoId },
                success: function (response) {
                    console.log("Resposta da exclusão:", response); // Log da resposta da exclusão
                    $('#excluirModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500);
                    }
                },
                error: function () {
                    showToast('Erro ao excluir o agendamento.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });
    }

    const meusDadosPage = document.getElementById('meusdadosPage');

    if (meusDadosPage) {
        const uploadInput = document.getElementById('file-upload');
        const uploadForm = document.getElementById('uploadLogoForm');
        const logoImage = document.querySelector(".barbearia-logo-img");
        const progressContainer = document.getElementById("uploadProgress");
        const progressBar = document.querySelector(".progress-bar-horizontal");
        const enderecoInput = document.getElementById("endereco");
        const numeroInput = document.getElementById("numero");
        const cepInput = document.getElementById("cep");
        const cidadeInput = document.querySelector("input[name='Cidade']");
        const estadoInput = document.querySelector("input[name='Estado']");
        const form = meusDadosPage.querySelector("form");

        // Concatenar endereço e número antes de enviar o formulário principal
        form.addEventListener("submit", function () {
            if (numeroInput.value) {
                enderecoInput.value = `${enderecoInput.value}, ${numeroInput.value}`;
            }
        });

        // Máscara de CEP (xxxxx-xxx)
        cepInput.addEventListener("input", function () {
            let cep = this.value.replace(/\D/g, ""); // Remove caracteres não numéricos
            if (cep.length > 5) {
                cep = cep.slice(0, 5) + "-" + cep.slice(5, 8); // Formato xxxxx-xxx
            }
            this.value = cep;
        });

        // Máscara de Telefone ((xx) xxxxx-xxxx)
        const telefoneInput = document.getElementById("telefone");
        telefoneInput.addEventListener("input", function () {
            let telefone = this.value.replace(/\D/g, ""); // Remove caracteres não numéricos
            if (telefone.length > 2) {
                telefone = "(" + telefone.slice(0, 2) + ") " + telefone.slice(2);
            }
            if (telefone.length > 9) {
                telefone = telefone.slice(0, 9) + "-" + telefone.slice(9, 13); // Formato (xx) xxxxx-xxxx
            }
            this.value = telefone;
        });

        // Função para buscar o endereço via API de CEP
        async function buscarCep(cep) {
            const cleanedCep = cep.replace(/\D/g, ""); // Remove caracteres não numéricos
            if (cleanedCep.length !== 8) {
                alert("Por favor, insira um CEP válido com 8 dígitos.");
                return;
            }

            const url = `https://viacep.com.br/ws/${cleanedCep}/json/`;
            try {
                const response = await fetch(url);
                const data = await response.json();

                if (data.erro) {
                    alert("CEP não encontrado.");
                    return;
                }

                // Preenche os campos com os dados do CEP
                enderecoInput.value = data.logradouro;
                cidadeInput.value = data.localidade;
                estadoInput.value = data.uf;
            } catch (error) {
                console.error("Erro ao buscar o CEP:", error);
            }
        }

        // Event listener para o campo de CEP
        cepInput.addEventListener("blur", function () {
            buscarCep(this.value);
        });

        // Upload do logo com barra de progresso horizontal
        uploadInput.addEventListener("change", function () {
            const file = this.files[0];
            if (file) {
                const formData = new FormData(uploadForm);
                formData.append("Logo", file);

                // Exibe a barra de progresso horizontal e reinicia a largura
                progressContainer.classList.remove("d-none");
                progressBar.style.width = "0%";

                fetch('/Barbearia/UploadLogo', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            logoImage.src = "data:image/png;base64," + data.newLogoBase64;
                            showToast("Logo atualizada com sucesso!", "success");
                        } else {
                            showToast("Erro ao atualizar a logo.", "danger");
                        }
                    })
                    .catch(error => {
                        showToast("Erro ao atualizar a logo.", "danger");
                        console.error("Erro:", error);
                    })
                    .finally(() => {
                        setTimeout(() => {
                            progressContainer.classList.add("d-none");
                        }, 1000);
                    });

                // Simulação do progresso
                let progress = 0;
                const interval = setInterval(() => {
                    if (progress < 100) {
                        progress += 10;
                        progressBar.style.width = `${progress}%`;
                    } else {
                        clearInterval(interval);
                    }
                }, 200);
            }
        });
    }


    // Página Feriados
    if ($('#feriadoPage').length > 0) {
        // Ação para o botão "Adicionar Feriado"
        $('#btnAdicionarFeriado').on('click', function () {
            $('#adicionarFeriadoNome').val('');
            $('#adicionarFeriadoData').val('');
            $('#adicionarModal').modal('show');
        });

        // Submissão do formulário de adição via AJAX
        $('#formAdicionarFeriado').on('submit', function (e) {
            e.preventDefault();
            const formData = $(this).serialize(); // Serializa os dados do formulário

            $.ajax({
                url: '/Feriado/Create',
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#adicionarModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao adicionar o feriado.", "danger");
                }
            });
        });

        // Ação para o botão "Editar"
        $('.btnEditarFeriado').on('click', function () {
            const feriadoId = $(this).data('id'); // Obtém o ID do feriado

            // Requisição para obter os detalhes do feriado
            $.get(`/Feriado/Details/${feriadoId}`, function (data) {
                if (data.success) {
                    // Popula o modal com os dados do feriado
                    $('#editarFeriadoId').val(data.feriadoBarbeariaId);
                    $('#editarDescricao').val(data.descricao);
                    $('#editarData').val(data.data.split('T')[0]); // Remove a parte de hora se existir
                    $('#editarRecorrente').val(data.recorrente.toString()); // Converte boolean para string
                    $('#editarModal').modal('show');
                } else {
                    showToast(data.message || "Erro ao carregar os detalhes do feriado.", "danger");
                }
            }).fail(function () {
                showToast("Erro ao buscar os detalhes do feriado.", "danger");
            });
        });

        // Submissão do formulário de edição via AJAX
        $('#formEditarFeriado').on('submit', function (e) {
            e.preventDefault();
            const formData = $(this).serialize();
            const feriadoId = $('#editarFeriadoId').val();

            $.ajax({
                url: `/Feriado/Edit/${feriadoId}`,
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#editarModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao editar o feriado.", "danger");
                }
            });
        });

        // Ação para o botão de excluir
        $('.btnExcluir').on('click', function () {
            const feriadoId = $(this).data('id');
            const feriadoNome = $(this).closest('.card').find('.card-title').text();
            $('#excluirFeriadoDescricao').text(feriadoNome);
            $('#btnConfirmarExcluir').data('id', feriadoId);
            $('#excluirModal').modal('show');
        });

        // Confirmação de exclusão
        $('#btnConfirmarExcluir').on('click', function () {
            const feriadoId = $(this).data('id');

            $.ajax({
                url: '/Feriado/DeleteConfirmed',
                type: 'POST',
                data: { id: feriadoId },
                success: function (response) {
                    $('#excluirModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao excluir o feriado.", "danger");
                }
            });
        });

        // Função de pesquisa
        $('#searchFeriado').on('input', function () {
            const query = $(this).val().toLowerCase(); // Texto da pesquisa em minúsculas
            const dateRegex = /^\d{2}\/\d{2}\/\d{4}$/; // Regex para validar formato de data dd/MM/yyyy

            $('.feriadoCard').each(function () {
                const text = $(this).text().toLowerCase(); // Texto completo do card
                const data = $(this).find('.feriadoCardText strong:contains("Data")').next().text(); // Texto da data no card

                let matches = false;
                if (dateRegex.test(query)) {
                    // Se a query for uma data, comparar diretamente
                    matches = data === query;
                } else {
                    // Caso contrário, pesquisar no texto geral
                    matches = text.includes(query);
                }

                $(this).toggle(matches); // Mostrar ou ocultar o card
            });
        });

        // Filtro por recorrência
        $('#filterRecorrente').on('change', function () {
            const filter = $(this).val(); // Valor do filtro: "", "true", ou "false"

            $('.feriadoCard').each(function () {
                const recorrente = $(this).data('recorrente')?.toString().toLowerCase(); // Converte para string minúscula
                const fixo = $(this).data('fixo'); // Valor booleano do atributo fixo
                let shouldShow = false;

                // Lógica de exibição baseada no filtro
                if (filter === "") {
                    // Mostrar todos os cards se o filtro estiver vazio
                    shouldShow = true;
                } else if (filter === "true") {
                    // Mostrar todos os feriados recorrentes (independente de ser true ou True) ou fixos
                    shouldShow = recorrente === "true" || fixo === true;
                } else if (filter === "false") {
                    // Mostrar apenas feriados não recorrentes
                    shouldShow = recorrente === "false";
                }

                // Aplica a lógica de exibição
                $(this).toggle(shouldShow);
            });
        });
    }


    // Página de Indisponibilidades
    if ($('#indisponibilidadePage').length > 0) {
        // Ação para o botão "Adicionar Indisponibilidade"
        $('#btnAdicionarIndisponibilidade').on('click', function () {
            carregarBarbeiros('#BarbeiroIdIndisponibilidade') // Carrega os barbeiros no select
                .then(() => {
                    $('#DataInicioIndisponibilidade').val('');
                    $('#DataFimIndisponibilidade').val('');
                    $('#MotivoIndisponibilidade').val('');
                    $('#adicionarIndisponibilidadeModal').modal('show');
                });
        });

        // Submissão do formulário de adição via AJAX
        $('#formAdicionarIndisponibilidade').on('submit', function (e) {
            e.preventDefault();
            const formData = $(this).serialize(); // Serializa os dados do formulário

            $.ajax({
                url: '/IndisponibilidadeBarbeiro/Create',
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#adicionarIndisponibilidadeModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao adicionar a indisponibilidade.", "danger");
                }
            });
        });

        // Ação para o botão de editar
        $('.btnEditarIndisponibilidade').on('click', function () {
            const indisponibilidadeId = $(this).data('id');

            // Primeiro, carregar os barbeiros no select
            carregarBarbeiros('#BarbeiroIdEditarIndisponibilidade')
                .then(() => {
                    $.get(`/IndisponibilidadeBarbeiro/Details/${indisponibilidadeId}`, function (data) {
                        $('#editarIndisponibilidadeId').val(data.indisponibilidadeId);
                        $('#BarbeiroIdEditarIndisponibilidade').val(data.barbeiroId); // Seleciona o barbeiro correspondente
                        $('#DataInicioEditarIndisponibilidade').val(data.dataInicio.replace(' ', 'T'));
                        $('#DataFimEditarIndisponibilidade').val(data.dataFim.replace(' ', 'T'));
                        $('#MotivoEditarIndisponibilidade').val(data.motivo);
                        $('#editarIndisponibilidadeModal').modal('show');
                    });
                });
        });

        // Submissão do formulário de edição via AJAX
        $('#formEditarIndisponibilidade').on('submit', function (e) {
            e.preventDefault();
            const formData = $(this).serialize();
            const indisponibilidadeId = $('#editarIndisponibilidadeId').val();

            $.ajax({
                url: `/IndisponibilidadeBarbeiro/Edit/${indisponibilidadeId}`,
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#editarIndisponibilidadeModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao editar a indisponibilidade.", "danger");
                }
            });
        });

        // Ação para o botão de excluir
        $('.btnExcluirIndisponibilidade').on('click', function () {
            const indisponibilidadeId = $(this).data('id');
            $('#btnConfirmarExcluirIndisponibilidade').data('id', indisponibilidadeId);
            $('#excluirIndisponibilidadeModal').modal('show');
        });

        // Confirmação de exclusão
        $('#btnConfirmarExcluirIndisponibilidade').on('click', function () {
            const indisponibilidadeId = $(this).data('id');

            $.ajax({
                url: '/IndisponibilidadeBarbeiro/DeleteConfirmed',
                type: 'POST',
                data: { id: indisponibilidadeId },
                success: function (response) {
                    $('#excluirIndisponibilidadeModal').modal('hide');
                    showToast(response.message, response.success ? "success" : "danger");
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function () {
                    showToast("Erro ao excluir a indisponibilidade.", "danger");
                }
            });
        });

        // Função para carregar os barbeiros no select
        function carregarBarbeiros(selectId) {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: '/Barbeiro/ObterBarbeirosPorBarbearia',
                    type: 'GET',
                    success: function (response) {
                        if (response.success) {
                            const select = $(selectId);
                            select.empty(); // Limpa o select antes de adicionar os novos valores
                            select.append('<option value="">Selecione um barbeiro</option>'); // Adiciona a opção padrão
                            response.barbeiros.forEach(barbeiro => {
                                select.append(`<option value="${barbeiro.barbeiroId}">${barbeiro.nome}</option>`);
                            });
                            resolve(); // Resolve a Promise após o sucesso
                        } else {
                            showToast(response.message, "warning");
                            reject(); // Rejeita a Promise caso haja erro
                        }
                    },
                    error: function () {
                        showToast("Erro ao carregar os barbeiros.", "danger");
                        reject(); // Rejeita a Promise caso haja erro
                    }
                });
            });
        }
    }

    if ($('#usuarioPage').length > 0) {

        function mostrarLoading() {
            $('#loadingSpinnerUsuario').show();
        }

        function ocultarLoading() {
            $('#loadingSpinnerUsuario').hide();
        }

        // Função para aplicar máscara de telefone
        function aplicarMascaraTelefone(input) {
            input.on('input', function () {
                let telefone = $(this).val().replace(/\D/g, ''); // Remove caracteres não numéricos

                // Limita o número de dígitos a 11
                if (telefone.length > 11) {
                    telefone = telefone.slice(0, 11);
                }

                // Aplica a máscara de acordo com o tamanho
                if (telefone.length > 10) {
                    telefone = telefone.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3'); // Formato com 11 dígitos
                } else if (telefone.length > 6) {
                    telefone = telefone.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3'); // Formato com 10 dígitos
                } else if (telefone.length > 2) {
                    telefone = telefone.replace(/(\d{2})(\d{0,5})/, '($1) $2'); // Formato inicial
                }

                $(this).val(telefone);
            });

            input.on('keypress', function (e) {
                // Impede a digitação de mais caracteres do que o necessário
                if ($(this).val().length >= 15 && e.keyCode !== 8 && e.keyCode !== 46) {
                    e.preventDefault();
                }
            });
        }

        // Aplicar a máscara de telefone ao campo de telefone
        aplicarMascaraTelefone($('#usuarioTelefone'));

        // Adicionar Usuário
        $('#btnAdicionarUsuario').on('click', function () {
            $('#usuarioId').val(''); // Limpa o campo do ID
            $('#usuarioNome').val('');
            $('#usuarioEmail').val('');
            $('#usuarioTelefone').val('');
            $('#usuarioRole').val('Admin');
            $('#usuarioStatus').val('1');
            $('#tipoUsuario').val('Admin'); // Adiciona tipo de usuário padrão
            $('#usuarioModalLabel').text('Adicionar Usuário');
            $('#usuarioModal').modal('show');
        });

        // Editar Usuário
        $(document).on('click', '.btnEditarUsuario', function () {
            const usuarioId = $(this).data('id');
            mostrarLoading();

            // Chama o método ObterUsuario
            $.get(`/Usuario/ObterUsuario/${usuarioId}`, function (response) {
                if (response.success) {
                    const usuario = response.data;
                    // Preenche os campos do modal com os dados do usuário
                    $('#usuarioId').val(usuario.usuarioId);
                    $('#usuarioNome').val(usuario.nome);
                    $('#usuarioEmail').val(usuario.email);
                    $('#usuarioTelefone').val(usuario.telefone);
                    $('#usuarioRole').val(usuario.role);
                    $('#usuarioStatus').val(usuario.status.toString());
                    $('#tipoUsuario').val(usuario.role === 'Barbeiro' ? 'Barbeiro' : 'Admin'); // Define tipo de usuário
                    $('#usuarioModalLabel').text('Editar Usuário');
                    $('#usuarioModal').modal('show');
                } else {
                    showToast(response.message, 'danger');
                }
            }).fail(function () {
                showToast('Erro ao buscar os dados do usuário.', 'danger');
            }).always(function () {
                ocultarLoading();
            });
        });

        $('#formUsuario').on('submit', function (e) {
            e.preventDefault();
            mostrarLoading();

            // Coleta os dados do formulário
            const formData = {
                UsuarioId: $('#usuarioId').val() || null, // Null se for novo
                Nome: $('#usuarioNome').val(),
                Email: $('#usuarioEmail').val(),
                Telefone: $('#usuarioTelefone').val(),
                Role: $('#usuarioRole').val(),
                Status: parseInt($('#usuarioStatus').val(), 10), // Converte para inteiro
                BarbeariaId: parseInt($('#BarbeariaId').val(), 10), // Converte para inteiro
                TipoUsuario: $('#usuarioRole').val() // Tipo de usuário: Admin ou Barbeiro
            };

            const url = formData.UsuarioId ? `/Usuario/Atualizar` : `/Usuario/Criar`; // Verifica se é PUT ou POST
            const method = formData.UsuarioId ? 'PUT' : 'POST';

            // Envia a solicitação AJAX
            $.ajax({
                url: url,
                type: method,
                contentType: 'application/json',
                data: JSON.stringify(formData), // Envia o JSON no corpo da solicitação
                success: function (response) {
                    $('#usuarioModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500); // Recarrega a página após sucesso
                    }
                },
                error: function () {
                    showToast('Erro ao salvar o usuário.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        // Deletar Usuário
        $(document).on('click', '.btnExcluirUsuario', function () {
            const usuarioId = $(this).data('id');
            const usuarioNome = $(this).closest('.usuario-card').find('p:first').text(); // Pega o primeiro <p> (nome do usuário)
            $('#excluirUsuarioNome').text(usuarioNome);
            $('#btnConfirmarExcluirUsuario').data('id', usuarioId);
            $('#excluirUsuarioModal').modal('show');
        });

        $('#btnConfirmarExcluirUsuario').on('click', function () {
            const usuarioId = $(this).data('id');
            mostrarLoading();

            $.ajax({
                url: `/Usuario/Deletar/${usuarioId}`,
                type: 'DELETE',
                success: function (response) {
                    $('#excluirUsuarioModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500);
                    }
                },
                error: function () {
                    showToast('Erro ao excluir o usuário.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });
    }


    var meusAgendamentosBarbeiro = document.getElementById('meusAgendamentosPage');

    if (meusAgendamentosBarbeiro) {
        // Controle de loading para o formulário de filtro
        var formFiltro = document.getElementById('formFiltro');
        var loadingOverlay = document.getElementById('loadingOverlay');
        var btnLimparFiltro = document.getElementById('btnLimparFiltro');
        var modalLimparFiltro = new bootstrap.Modal(document.getElementById('modalLimparFiltro'));

        if (formFiltro) {
            formFiltro.addEventListener('submit', function (e) {
                e.preventDefault(); // Previne o comportamento padrão do formulário

                // Mostra o overlay de carregamento
                if (loadingOverlay) {
                    loadingOverlay.style.display = 'block';
                }

                // Serializa os dados do formulário
                var formData = new FormData(formFiltro);
                var queryString = new URLSearchParams(formData).toString();

                // Faz a chamada AJAX para o filtro
                fetch(`/Barbeiro/FiltrarAgendamentos?${queryString}`)
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Erro ao buscar os agendamentos');
                        }
                        return response.json(); // Espera o JSON com os dados atualizados
                    })
                    .then(data => {
                        // Atualiza a tabela e os cartões com os novos dados
                        atualizarTabela(data.agendamentos);
                        atualizarCartoes(data.agendamentos);

                        // Esconde o overlay de carregamento
                        if (loadingOverlay) {
                            loadingOverlay.style.display = 'none';
                        }

                        // Exibe um toast de sucesso
                        showToast('Agendamentos filtrados com sucesso!', 'success');
                    })
                    .catch(error => {
                        console.error(error);

                        // Esconde o overlay de carregamento
                        if (loadingOverlay) {
                            loadingOverlay.style.display = 'none';
                        }

                        // Exibe um toast de erro
                        showToast('Erro ao filtrar os agendamentos. Tente novamente.', 'danger');
                    });
            });
        }

        // Evento para o botão de "Limpar Filtro"
        if (btnLimparFiltro) {
            btnLimparFiltro.addEventListener('click', function () {
                modalLimparFiltro.show();
            });
        }

        // Evento para confirmar a limpeza do filtro
        var confirmLimparFiltro = document.getElementById('confirmLimparFiltro');
        if (confirmLimparFiltro) {
            confirmLimparFiltro.addEventListener('click', function () {
                modalLimparFiltro.hide();

                // Reseta o formulário de filtro
                formFiltro.reset();

                // Envia o formulário com valores padrão
                formFiltro.dispatchEvent(new Event('submit'));
            });
        }

        // Limpa o overlay quando a página carregar
        window.addEventListener('load', function () {
            if (loadingOverlay) {
                loadingOverlay.style.display = 'none';
            }
        });

        // Função para atualizar a tabela
        function atualizarTabela(agendamentos) {
            var tabelaCorpo = document.querySelector('#tabelaMeusAgendamentos tbody');
            if (tabelaCorpo) {
                tabelaCorpo.innerHTML = ''; // Limpa o conteúdo atual

                agendamentos.forEach(agendamento => {
                    var linha = `
                <tr>
                    <td>${agendamento.cliente.nome}</td>
                    <td>${new Date(agendamento.dataHora).toLocaleString()}</td>
                    <td>${traduzirStatusAgendamento(agendamento.status)}</td>
                    <td>${agendamento.pagamento ? traduzirStatusPagamento(agendamento.pagamento.statusPagamento) : 'Sem pagamento'}</td>
                    <td>${agendamento.formaPagamento === 'creditCard' ? 'Cartão de Crédito' : 'Loja'}</td>
                    <td>${agendamento.precoTotal ? agendamento.precoTotal.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) : ''}</td>
                </tr>
            `;
                    tabelaCorpo.innerHTML += linha;
                });
            }
        }

        // Função para atualizar os cartões
        function atualizarCartoes(agendamentos) {
            var listaCartoes = document.querySelector('.agendamentos-list');
            if (listaCartoes) {
                listaCartoes.innerHTML = ''; // Limpa o conteúdo atual

                agendamentos.forEach(agendamento => {
                    var cartao = `
                <div class="agendamento-card" data-id="${agendamento.agendamentoId}">
                    <div class="agendamento-card-body">
                        <div class="agendamento-card-line">
                            <p><strong>Cliente:</strong> ${agendamento.cliente.nome}</p>
                        </div>
                        <div class="agendamento-card-line">
                            <p><strong>Data/Hora:</strong> ${new Date(agendamento.dataHora).toLocaleString()}</p>
                        </div>
                        <div class="agendamento-card-line">
                            <p><strong>Status Agendamento:</strong> ${traduzirStatusAgendamento(agendamento.status)}</p>
                        </div>
                        <div class="agendamento-card-line">
                            <p><strong>Status Pagamento:</strong> ${agendamento.pagamento ? traduzirStatusPagamento(agendamento.pagamento.statusPagamento) : 'Sem pagamento'}</p>
                        </div>
                        <div class="agendamento-card-line">
                            <p><strong>Forma de Pagamento:</strong> ${agendamento.formaPagamento === 'creditCard' ? 'Cartão de Crédito' : 'Loja'}</p>
                        </div>
                        <div class="agendamento-card-line">
                            <p><strong>Valor Total:</strong> ${agendamento.precoTotal ? agendamento.precoTotal.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) : ''}</p>
                        </div>
                    </div>
                </div>
            `;
                    listaCartoes.innerHTML += cartao;
                });
            }
        }

        // Função para traduzir status de agendamento
        function traduzirStatusAgendamento(status) {
            switch (status) {
                case 0: return 'Pendente';
                case 1: return 'Confirmado';
                case 2: return 'Cancelado';
                case 3: return 'Concluído';
                default: return 'Desconhecido';
            }
        }

        // Função para traduzir status de pagamento
        function traduzirStatusPagamento(statusPagamento) {
            switch (statusPagamento) {
                case 0: return 'Pendente';
                case 1: return 'Aprovado';
                case 2: return 'Recusado';
                case 3: return 'Cancelado';
                case 4: return 'Reembolsado';
                case 5: return 'Em Processamento';
                default: return 'Não Especificado';
            }
        }
    }



});
