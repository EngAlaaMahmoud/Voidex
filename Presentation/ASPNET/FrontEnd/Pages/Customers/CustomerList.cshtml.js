const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            deleteMode: false,
            customerGroupListLookupData: [],
            customerCategoryListLookupData: [],
            secondaryData: [],
            mainTitle: null,
            manageContactTitle: 'Manage Contact',
            id: '',
            name: '',
            number: '',
            customerGroupId: null,
            customerCategoryId: null,
            description: '',
            street: '',
            city: '',
            state: '',
            zipCode: '',
            country: '',
            phoneNumber: '',
            taxRegistrationNumber: '', // Changed from faxNumber
            emailAddress: '',
            website: '',
            whatsApp: '',
            linkedIn: '',
            facebook: '',
            instagram: '',
            twitterX: '',
            tikTok: '',
            errors: {
                name: '',
                customerGroupId: '',
                customerCategoryId: '',
                street: '',
                city: '',
                state: '',
                zipCode: '',
                country: '',
                phoneNumber: '',
                emailAddress: '',
            },
            isSubmitting: false
        });

        // Define all refs at the top level of setup function
        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const manageContactModalRef = Vue.ref(null);
        const secondaryGridRef = Vue.ref(null);
        const nameRef = Vue.ref(null);
        const numberRef = Vue.ref(null);
        const streetRef = Vue.ref(null);
        const cityRef = Vue.ref(null);
        const stateRef = Vue.ref(null);
        const zipCodeRef = Vue.ref(null);
        const countryRef = Vue.ref(null);
        const phoneNumberRef = Vue.ref(null);
        const taxRegistrationNumberRef = Vue.ref(null); // Changed from faxNumberRef
        const emailAddressRef = Vue.ref(null);
        const websiteRef = Vue.ref(null);
        const whatsAppRef = Vue.ref(null);
        const linkedInRef = Vue.ref(null);
        const facebookRef = Vue.ref(null);
        const instagramRef = Vue.ref(null);
        const twitterXRef = Vue.ref(null);
        const tikTokRef = Vue.ref(null);
        const customerGroupIdRef = Vue.ref(null);
        const customerCategoryIdRef = Vue.ref(null);

        const validateForm = function () {
            state.errors.name = '';
            state.errors.customerGroupId = '';
            state.errors.customerCategoryId = '';
            state.errors.street = '';
            state.errors.city = '';
            state.errors.state = '';
            state.errors.zipCode = '';
            state.errors.country = '';
            state.errors.phoneNumber = '';
            state.errors.emailAddress = '';

            let isValid = true;

            if (!state.name) {
                state.errors.name = 'Name is required.';
                isValid = false;
            }
            if (!state.phoneNumber) {
                state.errors.phoneNumber = 'Phone Number is required.';
                isValid = false;
            }

            return isValid;
        };

        const resetFormState = () => {
            state.id = '';
            state.number = '';
            state.name = '';
            state.customerGroupId = null;
            state.customerCategoryId = null;
            state.description = '';
            state.street = '';
            state.city = '';
            state.state = '';
            state.zipCode = '';
            state.country = '';
            state.phoneNumber = '';
            state.taxRegistrationNumber = ''; // Changed from faxNumber
            state.emailAddress = '';
            state.website = '';
            state.whatsApp = '';
            state.linkedIn = '';
            state.facebook = '';
            state.instagram = '';
            state.twitterX = '';
            state.tikTok = '';
            state.errors = {
                name: '',
                customerGroupId: '',
                customerCategoryId: '',
                street: '',
                city: '',
                state: '',
                zipCode: '',
                country: '',
                phoneNumber: '',
                emailAddress: '',
            };
        };

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/Customer/GetCustomerList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            createMainData: async (name, description, phoneNumber, taxRegistrationNumber, createdById) => {
                try {
                    const response = await AxiosManager.post('/Customer/CreateCustomer', {
                        name,
                        description,
                        phoneNumber,
                        taxRegistrationNumber,
                        createdById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateMainData: async (id, name, description, phoneNumber, taxRegistrationNumber, updatedById) => {
                try {
                    console.log('Update Customer Data:', {
                        id,
                        name,
                        description,
                        phoneNumber,
                        taxRegistrationNumber,
                        updatedById
                    });

                    const response = await AxiosManager.post('/Customer/UpdateCustomer', {
                        id,
                        name,
                        description,
                        phoneNumber,
                        taxRegistrationNumber,
                        updatedById
                    });

                    console.log('Update Response:', response.data);
                    return response;
                } catch (error) {
                    console.error('Update Customer Error:', error);
                    throw error;
                }
            },
            deleteMainData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/Customer/DeleteCustomer', {
                        id,
                        deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },

            getSecondaryData: async (customerId) => {
                try {
                    const response = await AxiosManager.get('/CustomerContact/GetCustomerContactByCustomerIdList?customerId=' + customerId, {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            createSecondaryData: async (name, jobTitle, phoneNumber, emailAddress, description, customerId, createdById) => {
                try {
                    const response = await AxiosManager.post('/CustomerContact/CreateCustomerContact', {
                        name, jobTitle, phoneNumber, emailAddress, description, customerId, createdById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateSecondaryData: async (id, name, jobTitle, phoneNumber, emailAddress, description, customerId, updatedById) => {
                try {
                    const response = await AxiosManager.post('/CustomerContact/UpdateCustomerContact', {
                        id, name, jobTitle, phoneNumber, emailAddress, description, customerId, updatedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            deleteSecondaryData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/CustomerContact/DeleteCustomerContact', {
                        id, deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
        };

        const methods = {
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = response?.data?.content?.data.map(item => ({
                    ...item,
                    createdAtUtc: new Date(item.createdAtUtc)
                }));
            },
            populateSecondaryData: async (customerId) => {
                const response = await services.getSecondaryData(customerId);
                state.secondaryData = response?.data?.content?.data.map(item => ({
                    ...item,
                    createdAtUtc: new Date(item.createdAtUtc)
                }));
            },
        };

        // Text box definitions
        const nameText = {
            obj: null,
            create: () => {
                nameText.obj = new ej.inputs.TextBox({
                    placeholder: 'Enter Name',
                });
                nameText.obj.appendTo(nameRef.value);
            },
            refresh: () => {
                if (nameText.obj) {
                    nameText.obj.value = state.name;
                }
            }
        };

        const numberText = {
            obj: null,
            create: () => {
                numberText.obj = new ej.inputs.TextBox({
                    placeholder: '[auto]',
                    readonly: true
                });
                numberText.obj.appendTo(numberRef.value);
            },
            refresh: () => {
                if (numberText.obj) {
                    numberText.obj.value = state.number;
                }
            }
        };

        const phoneNumberText = {
            obj: null,
            create: () => {
                phoneNumberText.obj = new ej.inputs.TextBox({
                    placeholder: 'Enter Phone Number',
                });
                phoneNumberText.obj.appendTo(phoneNumberRef.value);
            },
            refresh: () => {
                if (phoneNumberText.obj) {
                    phoneNumberText.obj.value = state.phoneNumber;
                }
            }
        };

        const taxRegistrationNumberText = {
            obj: null,
            create: () => {
                taxRegistrationNumberText.obj = new ej.inputs.TextBox({
                    placeholder: 'Enter Tax Registration Number',
                });
                taxRegistrationNumberText.obj.appendTo(taxRegistrationNumberRef.value);
            },
            refresh: () => {
                if (taxRegistrationNumberText.obj) {
                    taxRegistrationNumberText.obj.value = state.taxRegistrationNumber;
                }
            }
        };

        // Watchers
        Vue.watch(
            () => state.name,
            (newVal, oldVal) => {
                state.errors.name = '';
                nameText.refresh();
            }
        );

        Vue.watch(
            () => state.phoneNumber,
            (newVal, oldVal) => {
                state.errors.phoneNumber = '';
                phoneNumberText.refresh();
            }
        );

        Vue.watch(
            () => state.taxRegistrationNumber,
            (newVal, oldVal) => {
                taxRegistrationNumberText.refresh();
            }
        );

        const handler = {
            handleSubmit: async function () {
                try {
                    state.isSubmitting = true;
                    await new Promise(resolve => setTimeout(resolve, 200));

                    if (!validateForm()) {
                        state.isSubmitting = false;
                        return;
                    }

                    const response = state.id === ''
                        ? await services.createMainData(
                            state.name,
                            state.description,
                            state.phoneNumber,
                            state.taxRegistrationNumber,
                            StorageManager.getUserId()
                        )
                        : state.deleteMode
                            ? await services.deleteMainData(state.id, StorageManager.getUserId())
                            : await services.updateMainData(
                                state.id,
                                state.name,
                                state.description,
                                state.phoneNumber,
                                state.taxRegistrationNumber,
                                StorageManager.getUserId()
                            );

                    if (response.data.code === 200) {
                        await methods.populateMainData();
                        mainGrid.refresh();

                        if (!state.deleteMode) {
                            // Update the state from response data
                            const responseData = response?.data?.content?.data;
                            state.id = responseData?.id ?? '';
                            state.number = responseData?.number ?? '';
                            state.name = responseData?.name ?? '';
                            state.description = responseData?.description ?? '';
                            state.phoneNumber = responseData?.phoneNumber ?? '';
                            state.taxRegistrationNumber = responseData?.taxRegistrationNumber ?? '';

                            Swal.fire({
                                icon: 'success',
                                title: 'Save Successful',
                                timer: 1000,
                                showConfirmButton: false
                            });
                        } else {
                            Swal.fire({
                                icon: 'success',
                                title: 'Delete Successful',
                                text: 'Form will be closed...',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            setTimeout(() => {
                                mainModal.obj.hide();
                                resetFormState();
                            }, 2000);
                        }

                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: state.deleteMode ? 'Delete Failed' : 'Save Failed',
                            text: response.data.message ?? 'Please check your data.',
                            confirmButtonText: 'Try Again'
                        });
                    }

                } catch (error) {
                    console.error('Form submission error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'An Error Occurred',
                        text: error.response?.data?.message ?? 'Please try again.',
                        confirmButtonText: 'OK'
                    });
                } finally {
                    state.isSubmitting = false;
                }
            },
        };

        const mainGrid = {
            obj: null,
            create: async (dataSource) => {
                mainGrid.obj = new ej.grids.Grid({
                    height: '240px',
                    dataSource: dataSource,
                    allowFiltering: true,
                    allowSorting: true,
                    allowSelection: true,
                    allowGrouping: true,
                    allowTextWrap: true,
                    allowResizing: true,
                    allowPaging: true,
                    allowExcelExport: true,
                    filterSettings: { type: 'CheckBox' },
                    sortSettings: { columns: [{ field: 'createdAtUtc', direction: 'Descending' }] },
                    pageSettings: { currentPage: 1, pageSize: 50, pageSizes: ["10", "20", "50", "100", "200", "All"] },
                    selectionSettings: { persistSelection: true, type: 'Single' },
                    autoFit: true,
                    showColumnMenu: true,
                    gridLines: 'Horizontal',
                    columns: [
                        { type: 'checkbox', width: 60 },
                        {
                            field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false
                        },
                        { field: 'number', headerText: 'Number', width: 150, minWidth: 150 },
                        { field: 'name', headerText: 'Name', width: 200, minWidth: 200 },
                        { field: 'phoneNumber', headerText: 'Phone', width: 200, minWidth: 200 },
                        { field: 'taxRegistrationNumber', headerText: 'Tax Reg. No.', width: 200, minWidth: 200 },
                        { field: 'createdAtUtc', headerText: 'Created At UTC', width: 150, format: 'yyyy-MM-dd HH:mm' }
                    ],
                    toolbar: [
                        'ExcelExport', 'Search',
                        { type: 'Separator' },
                        { text: 'Add', tooltipText: 'Add', prefixIcon: 'e-add', id: 'AddCustom' },
                        { text: 'Edit', tooltipText: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' },
                        { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' },
                        { type: 'Separator' },
                        { text: 'Manage Contact', tooltipText: 'Manage Contact', id: 'ManageContactCustom' },
                    ],
                    beforeDataBound: () => { },
                    dataBound: function () {
                        mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'ManageContactCustom'], false);
                        mainGrid.obj.autoFitColumns(['name', 'phoneNumber', 'taxRegistrationNumber', 'createdAtUtc']);
                    },
                    excelExportComplete: () => { },
                    rowSelected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'ManageContactCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'ManageContactCustom'], false);
                        }
                    },
                    rowDeselected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'ManageContactCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'ManageContactCustom'], false);
                        }
                    },
                    rowSelecting: () => {
                        if (mainGrid.obj.getSelectedRecords().length) {
                            mainGrid.obj.clearSelection();
                        }
                    },
                    toolbarClick: async (args) => {
                        if (args.item.id === 'MainGrid_excelexport') {
                            mainGrid.obj.excelExport();
                        }

                        if (args.item.id === 'AddCustom') {
                            state.deleteMode = false;
                            state.mainTitle = 'Add Customer';
                            resetFormState();
                            mainModal.obj.show();
                        }

                        if (args.item.id === 'EditCustom') {
                            state.deleteMode = false;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Edit Customer';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.description = selectedRecord.description ?? '';
                                state.phoneNumber = selectedRecord.phoneNumber ?? '';
                                state.taxRegistrationNumber = selectedRecord.taxRegistrationNumber ?? '';
                                mainModal.obj.show();
                            }
                        }

                        if (args.item.id === 'DeleteCustom') {
                            state.deleteMode = true;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Delete Customer?';
                                state.id = selectedRecord.id ?? '';
                                mainModal.obj.show();
                            }
                        }

                        if (args.item.id === 'ManageContactCustom') {
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.id = selectedRecord.id ?? '';
                                state.manageContactTitle = 'Manage Contact';
                                await methods.populateSecondaryData(state.id);
                                secondaryGrid.refresh();
                                manageContactModal.obj.show();
                            }
                        }
                    }
                });

                mainGrid.obj.appendTo(mainGridRef.value);
            },
            refresh: () => {
                mainGrid.obj.setProperties({ dataSource: state.mainData });
            }
        };

        const mainModal = {
            obj: null,
            create: () => {
                mainModal.obj = new bootstrap.Modal(mainModalRef.value, {
                    backdrop: 'static',
                    keyboard: false
                });
            }
        };

        const manageContactModal = {
            obj: null,
            create: () => {
                manageContactModal.obj = new bootstrap.Modal(manageContactModalRef.value, {
                    backdrop: 'static',
                    keyboard: false
                });
            }
        };

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['Customers']);
                await SecurityManager.validateToken();

                await methods.populateMainData();
                await mainGrid.create(state.mainData);

                mainModal.create();
                manageContactModal.create();

                // Initialize text boxes
                nameText.create();
                numberText.create();
                phoneNumberText.create();
                taxRegistrationNumberText.create();

            } catch (e) {
                console.error('page init error:', e);
            }
        });

        return {
            mainGridRef,
            mainModalRef,
            manageContactModalRef,
            secondaryGridRef,
            nameRef,
            numberRef,
            phoneNumberRef,
            taxRegistrationNumberRef,
            state,
            handler,
        };
    }
};

Vue.createApp(App).mount('#app');