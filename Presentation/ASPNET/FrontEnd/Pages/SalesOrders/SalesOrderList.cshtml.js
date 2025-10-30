const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            deleteMode: false,
            customerListLookupData: [],
            //salesOrderStatusListLookupData: [],
            taxListLookupData: [],
            secondaryData: [],
            productListLookupData: [],
            mainTitle: null,
            id: '',
            number: '',
            orderDate: '',
            description: '',
            customerId: null,
            displayDiscount: '0.00', // Formatted discount for display           // orderStatus: null,
            taxId: null,
            barcode: '',
            errors: {
                orderDate: '',
                customerId: '',
               // orderStatus: '',
                description: ''
            },
            showComplexDiv: false,
            isSubmitting: false,
            subTotalAmount: '0.00',
            vatAmount: '0.00',
            withholdingAmount: '0.00',
            //discount: '0.00',
            discount: 0, // Changed from string to number for calculations
            totalAmount: '0.00'
        });

        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const orderDateRef = Vue.ref(null);
        const numberRef = Vue.ref(null);
        const customerIdRef = Vue.ref(null);
       // const orderStatusRef = Vue.ref(null);
        const secondaryGridRef = Vue.ref(null);
        const barcodeRef = Vue.ref(null);
        const taxIdRef = Vue.ref(null);

        const validateForm = function () {
            state.errors.orderDate = '';
            state.errors.customerId = '';
         //   state.errors.orderStatus = '';

            let isValid = true;

            if (!state.customerId) {
                state.errors.customerId = 'Customer is required.';
                isValid = false;
            }
            //if (!state.orderStatus) {
            //    state.errors.orderStatus = 'Order status is required.';
            //    isValid = false;
            //}

            return isValid;
        };

        const resetFormState = () => {
            state.id = '';
            state.number = '';
            state.orderDate = '';
            state.description = '';
            state.customerId = null;
          //  state.orderStatus = null;
            state.taxId = null;
            state.barcode = '';
            state.errors = {
                orderDate: '',
                customerId: '',
              //  orderStatus: '',
                description: ''
            };
            state.secondaryData = [];
            state.subTotalAmount = '0.00';
            state.vatAmount = '0.00';
            state.withholdingAmount = '0.00';
            //state.discount = '0.00';
            state.discount = 0;
            state.totalAmount = '0.00';
            state.showComplexDiv = false;
            state.displayDiscount = '0.00';
        };

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/SalesOrder/GetSalesOrderList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            //createMainData: async (description, customerId, taxId, createdById, discount) => {
            //    try {
            //        const response = await AxiosManager.post('/SalesOrder/CreateSalesOrder', {
            //            description, customerId, taxId, createdById, discount
            //        });
            //        return response;
            //    } catch (error) {
            //        throw error;
            //    }
            //},
            createMainData: async (description, customerId, taxId, createdById, discount) => {
                try {                   
                    const response = await AxiosManager.post('/SalesOrder/CreateSalesOrder', {
                        Description: description,
                        CustomerId: customerId,
                        TaxId: taxId,
                        CreatedById: createdById,
                        Discount: discount
                    }, {
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateMainData: async (id, description, customerId, taxId, updatedById, discount) => {
                try {
                    const response = await AxiosManager.post('/SalesOrder/UpdateSalesOrder', {
                        id, description, customerId, taxId, updatedById, discount
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            deleteMainData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/SalesOrder/DeleteSalesOrder', {
                        id, deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getCustomerListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/Customer/GetCustomerList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            //getSalesOrderStatusListLookupData: async () => {
            //    try {
            //        const response = await AxiosManager.get('/SalesOrder/GetSalesOrderStatusList', {});
            //        return response;
            //    } catch (error) {
            //        throw error;
            //    }
            //},
            getTaxListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/Tax/GetTaxList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getSecondaryData: async (salesOrderId) => {
                try {
                    const response = await AxiosManager.get('/SalesOrderItem/GetSalesOrderItemBySalesOrderIdList?salesOrderId=' + salesOrderId, {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            createSecondaryData: async (unitPrice, quantity, summary, productId, salesOrderId, createdById) => {
                try {
                    const response = await AxiosManager.post('/SalesOrderItem/CreateSalesOrderItem', {
                        unitPrice, quantity, summary, productId, salesOrderId, createdById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateSecondaryData: async (id, unitPrice, quantity, summary, productId, salesOrderId, updatedById) => {
                try {
                    const response = await AxiosManager.post('/SalesOrderItem/UpdateSalesOrderItem', {
                        id, unitPrice, quantity, summary, productId, salesOrderId, updatedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            deleteSecondaryData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/SalesOrderItem/DeleteSalesOrderItem', {
                        id, deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getProductListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/Product/GetProductList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getProductByNumber: async (number) => {
                try {
                    const response = await AxiosManager.get('/Product/GetProductByNumber?number=' + number, {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateSalesOrderDiscount: async (id, discount, updatedById) => {
                try {
                    const response = await AxiosManager.post('/SalesOrder/UpdateSalesOrderDiscount', {
                        id, discount, updatedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },        
            getProductByBarcode: async (barcode) => {
                try {
                    const response = await AxiosManager.get(`/Product/GetProductByBarcode?barcode=${encodeURIComponent(barcode)}`, {});
                    return response;
                } catch (error) {
                    throw error;
                }
            }
        };

        const methods = {
            populateCustomerListLookupData: async () => {
                const response = await services.getCustomerListLookupData();
                state.customerListLookupData = response?.data?.content?.data;
            },
            //populateSalesOrderStatusListLookupData: async () => {
            //    const response = await services.getSalesOrderStatusListLookupData();
            //    state.salesOrderStatusListLookupData = response?.data?.content?.data;
            //},
            populateTaxListLookupData: async () => {
                const response = await services.getTaxListLookupData();
                state.taxListLookupData = response?.data?.content?.data;
            },
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = response?.data?.content?.data.map(item => ({
                    ...item,
                    orderDate: new Date(item.orderDate),
                    createdAtUtc: new Date(item.createdAtUtc)
                }));
            },
            populateSecondaryData: async (salesOrderId) => {
                try {
                    const response = await services.getSecondaryData(salesOrderId);
                    state.secondaryData = response?.data?.content?.data.map(item => ({
                        ...item,
                        createdAtUtc: new Date(item.createdAtUtc)
                    }));
                    methods.refreshPaymentSummary(salesOrderId);
                } catch (error) {
                    state.secondaryData = [];
                }
            },
            populateProductListLookupData: async () => {
                const response = await services.getProductListLookupData();
                state.productListLookupData = response?.data?.content?.data;
            },
            addProductByBarcode: async () => {
                if (!state.barcode) return;

                try {
                    let product = null;
                    product = state.productListLookupData.find(p =>
                        p.barcode && p.barcode.toLowerCase() === state.barcode.toLowerCase()
                    );

                    if (!product) {
                        const response = await services.getProductByBarcode(state.barcode);
                        if (response.data.code === 200 && response.data.content.data) {
                            product = response.data.content.data;
                            const existingProduct = state.productListLookupData.find(p => p.id === product.id);
                            if (!existingProduct) {
                                state.productListLookupData.push(product);
                            }
                        }
                    }

                    if (product) {
                        const salesOrderId = state.id;
                        const userId = StorageManager.getUserId();
                        const unitPrice = product.unitPrice || 0;
                        const quantity = 1;
                        const summary = product.description || '';
                        const productId = product.id;

                        const vatPercentage = product.vatPercentage || 0;
                        const vatAmount = unitPrice * (vatPercentage / 100);
                        const total = (unitPrice + vatAmount) * quantity;

                        await services.createSecondaryData(unitPrice, quantity, summary, productId, salesOrderId, userId);
                        await methods.populateSecondaryData(salesOrderId);
                        secondaryGrid.refresh();

                        state.barcode = '';
                        if (barcodeRef.value) {
                            barcodeRef.value.focus();
                        }

                        Swal.fire({
                            icon: 'success',
                            title: 'Product Added',
                            text: `Added ${product.name} via barcode`,
                            timer: 1000,
                            showConfirmButton: false
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Product Not Found',
                            text: 'No product found with this barcode.',
                            confirmButtonText: 'OK'
                        });
                    }
                } catch (error) {
                    console.error('Barcode scan error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Failed to add product. Please try again.',
                        confirmButtonText: 'OK'
                    });
                }
            },

            //refreshPaymentSummary: async (id) => {
            //    const record = state.mainData.find(item => item.id === id);
            //    if (record) {
            //        console.log('Record found for payment summary:', record);
            //        state.subTotalAmount = NumberFormatManager.formatToLocale(record.beforeTaxAmount ?? 0);
            //        state.vatAmount = NumberFormatManager.formatToLocale(record.vatAmount ?? 0);
            //        state.withholdingAmount = NumberFormatManager.formatToLocafle(record.withholdingAmount ?? 0);
            //        //state.discount = NumberFormatManager.formatToLocale(record.discount ?? 0);
            //        state.discount = record.discount ?? 0;
            //        state.totalAmount = NumberFormatManager.formatToLocale(record.afterTaxAmount ?? 0);
            //        state.discount = NumberFormatManager.formatToLocale(record.discount ?? 0); // Format discount
            //    }
            //},

            refreshPaymentSummary: async (id) => {
                console.log('NumberFormatManager available:', typeof NumberFormatManager);
                console.log('formatToLocale available:', typeof NumberFormatManager.formatToLocale);

                const record = state.mainData.find(item => item.id === id);
                if (record) {
                    console.log('Record found for payment summary:', record);

                    try {
                        state.subTotalAmount = NumberFormatManager.formatToLocale(record.beforeTaxAmount ?? 0);
                        state.vatAmount = NumberFormatManager.formatToLocale(record.vatAmount ?? 0);
                        state.withholdingAmount = NumberFormatManager.formatToLocale(record.withholdingAmount ?? 0);
                        state.discount = record.discount ?? 0;
                        state.totalAmount = NumberFormatManager.formatToLocale(record.afterTaxAmount ?? 0);

                        // Update display discount
                        methods.formatDisplayDiscount();

                        console.log('Payment summary updated successfully');
                    } catch (error) {
                        console.error('Error in payment summary:', error);
                        // Fallback to basic formatting
                        state.subTotalAmount = (record.beforeTaxAmount ?? 0).toString();
                        state.vatAmount = (record.vatAmount ?? 0).toString();
                        state.withholdingAmount = (record.withholdingAmount ?? 0).toString();
                        state.discount = record.discount ?? 0;
                        state.totalAmount = (record.afterTaxAmount ?? 0).toString();
                        methods.formatDisplayDiscount();
                    }
                } else {
                    console.log('No record found for id:', id);
                    // Set default values
                    state.subTotalAmount = '0.00';
                    state.vatAmount = '0.00';
                    state.withholdingAmount = '0.00';
                    state.discount = 0;
                    state.displayDiscount = '0.00';
                    state.totalAmount = '0.00';
                }
            },
            //refreshPaymentSummary: async (id) => {
            //    console.log('NumberFormatManager available:', typeof NumberFormatManager);
            //    console.log('formatToLocale available:', typeof NumberFormatManager.formatToLocale);

            //    const record = state.mainData.find(item => item.id === id);
            //    if (record) {
            //        console.log('Record found for payment summary:', record);

            //        try {
            //            state.subTotalAmount = NumberFormatManager.formatToLocale(record.beforeTaxAmount ?? 0);
            //            state.vatAmount = NumberFormatManager.formatToLocale(record.vatAmount ?? 0);
            //            state.withholdingAmount = NumberFormatManager.formatToLocale(record.withholdingAmount ?? 0);
            //            //state.discount = record.discount ?? 0;
            //            state.totalAmount = NumberFormatManager.formatToLocale(record.afterTaxAmount ?? 0);

            //            state.discount = record.discount ?? 0; // Don't format this one
            //            state.discountDisplay = NumberFormatManager.formatToLocale(record.discount ?? 0);
            //            console.log('Payment summary updated successfully');
            //        } catch (error) {
            //            console.error('Error in payment summary:', error);
            //            // Fallback to basic formatting
            //            state.subTotalAmount = (record.beforeTaxAmount ?? 0).toString();
            //            state.vatAmount = (record.vatAmount ?? 0).toString();
            //            state.withholdingAmount = (record.withholdingAmount ?? 0).toString();
            //            state.discount = record.discount ?? 0;
            //            state.totalAmount = (record.afterTaxAmount ?? 0).toString();

            //        }
            //    } else {
            //        console.log('No record found for id:', id);
            //        // Set default values
            //        state.subTotalAmount = '0.00';
            //        state.vatAmount = '0.00';
            //        state.withholdingAmount = '0.00';
            //        state.discount = 0;
            //        state.totalAmount = '0.00';
            //    }
            //},

            updateDiscount: async () => {
                if (!state.id) return;

                try {
                    // Validate discount is a positive number
                    const discountValue = parseFloat(state.discount) || 0;
                    if (discountValue < 0) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Invalid Discount',
                            text: 'Discount cannot be negative.',
                            confirmButtonText: 'OK'
                        });
                        state.discount = 0;
                        return;
                    }

                    state.isSubmitting = true;

                    // Update discount via API
                    const response = await services.updateSalesOrderDiscount(
                        state.id,
                        discountValue,
                        StorageManager.getUserId()
                    );
                    console.log('test222', response)

                    if (response.status === 200) {
                        await methods.populateMainData();
                        await methods.refreshPaymentSummary(state.id);

                        Swal.fire({
                            icon: 'success',
                            title: 'Discount Updated',
                            timer: 1000,
                            showConfirmButton: false
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Update Failed',
                            text: response.data.message ?? 'Failed to update discount.',
                            confirmButtonText: 'Try Again'
                        });
                    }
                } catch (error) {
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

            //updateDiscount: async () => {
            //    if (!state.id) return;

            //    try {
            //        // Validate discount is a positive number
            //        const discountValue = parseFloat(state.discount) || 0;
            //        if (discountValue < 0) {
            //            Swal.fire({
            //                icon: 'error',
            //                title: 'Invalid Discount',
            //                text: 'Discount cannot be negative.',
            //                confirmButtonText: 'OK'
            //            });
            //            state.discount = 0;
            //            return;
            //        }

            //        state.isSubmitting = true;

            //        // Update discount via API
            //        const response = await services.updateSalesOrderDiscount(
            //            state.id,
            //            discountValue,
            //            StorageManager.getUserId()
            //        );
            //        console.log('test222', response)

            //        if (response.status === 200) {
            //            await methods.populateMainData();
            //            await methods.refreshPaymentSummary(state.id);
            //            console.log('test', response)
            //            Swal.fire({
            //                icon: 'success',
            //                title: 'Discount Updated',
            //                timer: 1000,
            //                showConfirmButton: false
            //            });
            //        } else {
            //            Swal.fire({
            //                icon: 'error',
            //                title: 'Update Failed',
            //                text: response.data.message ?? 'Failed to update discount.',
            //                confirmButtonText: 'Try Again'
            //            });
            //        }
            //    } catch (error) {
            //        Swal.fire({
            //            icon: 'error',
            //            title: 'An Error Occurred',
            //            text: error.response?.data?.message ?? 'Please try again.',
            //            confirmButtonText: 'OK'
            //        });
            //    } finally {
            //        state.isSubmitting = false;
            //    }
            //},
            handleFormSubmit: async () => {
                state.isSubmitting = true;
                await new Promise(resolve => setTimeout(resolve, 200));

                if (!validateForm()) {
                    state.isSubmitting = false;
                    return;
                }

                try {
                    console.log('Form State:', {
                        description: state.description,
                        customerId: state.customerId,
                        taxId: state.taxId,
                        discount: state.discount,
                        createdById: StorageManager.getUserId()
                    });
                    const response = state.id === ''
                        ? await services.createMainData(state.description, state.customerId, state.taxId, StorageManager.getUserId(), state.discount)
                        : state.deleteMode
                            ? await services.deleteMainData(state.id, StorageManager.getUserId())
                            : await services.updateMainData(state.id, state.description, state.customerId, state.taxId, StorageManager.getUserId(), state.discount);
                                                         //id, orderDate, description, customerId, taxId, updatedById, discount
                    if (response.data.code === 200) {
                        await methods.populateMainData();
                        mainGrid.refresh();

                        if (!state.deleteMode) {
                            state.mainTitle = 'Edit Sales Order';
                            state.id = response?.data?.content?.data.id ?? '';
                            state.number = response?.data?.content?.data.number ?? '';
                            //state.orderDate = response?.data?.content?.data.orderDate ? new Date(response.data.content.data.orderDate) : null;
                            state.description = response?.data?.content?.data.description ?? '';
                            state.customerId = response?.data?.content?.data.customerId ?? '';
                            //state.orderStatus = String(response?.data?.content?.data.orderStatus ?? '');
                            state.discount = response?.data?.content?.data.discount ?? 0;
                            state.taxId = response?.data?.content?.data.taxId ?? null;
                            state.showComplexDiv = true;
                            taxListLookup.trackingChange = true;
                            await methods.refreshPaymentSummary(state.id);

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
                    console.error('Error response:', error.response);
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
            onMainModalHidden: () => {
                state.errors.orderDate = '';
                state.errors.customerId = '';
               // state.errors.orderStatus = '';
            }
        };

        const customerListLookup = {
            obj: null,
            create: () => {
                if (state.customerListLookupData && Array.isArray(state.customerListLookupData)) {
                    customerListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.customerListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a Customer',
                        filterBarPlaceholder: 'Search',
                        sortOrder: 'Ascending',
                        allowFiltering: true,
                        filtering: (e) => {
                            e.preventDefaultAction = true;
                            let query = new ej.data.Query();
                            if (e.text !== '') {
                                query = query.where('name', 'startsWith', e.text, true);
                            }
                            e.updateData(state.customerListLookupData, query);
                        },
                        change: (e) => {
                            state.customerId = e.value;
                        }
                    });
                    customerListLookup.obj.appendTo(customerIdRef.value);
                }
            },
            refresh: () => {
                if (customerListLookup.obj) {
                    customerListLookup.obj.value = state.customerId;
                }
            }
        };

        const taxListLookup = {
            obj: null,
            trackingChange: false,
            create: () => {
                if (state.taxListLookupData && Array.isArray(state.taxListLookupData)) {
                    taxListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.taxListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a Tax',
                        change: async (e) => {
                            state.taxId = e.value;
                            if (e.isInteracted && taxListLookup.trackingChange) {
                                await methods.handleFormSubmit();
                            }
                        }
                    });
                    taxListLookup.obj.appendTo(taxIdRef.value);
                }
            },
            refresh: () => {
                if (taxListLookup.obj) {
                    taxListLookup.obj.value = state.taxId;
                }
            }
        };

        //const salesOrderStatusListLookup = {
        //    obj: null,
        //    create: () => {
        //        if (state.salesOrderStatusListLookupData && Array.isArray(state.salesOrderStatusListLookupData)) {
        //            salesOrderStatusListLookup.obj = new ej.dropdowns.DropDownList({
        //                dataSource: state.salesOrderStatusListLookupData,
        //                fields: { value: 'id', text: 'name' },
        //                placeholder: 'Select an Order Status',
        //                change: (e) => {
        //                    state.orderStatus = e.value;
        //                }
        //            });
        //            salesOrderStatusListLookup.obj.appendTo(orderStatusRef.value);
        //        }
        //    },
        //    refresh: () => {
        //        if (salesOrderStatusListLookup.obj) {
        //            salesOrderStatusListLookup.obj.value = state.orderStatus;
        //        }
        //    }
        //};

        const orderDatePicker = {
            obj: null,
            create: () => {
                orderDatePicker.obj = new ej.calendars.DatePicker({
                    format: 'yyyy-MM-dd',
                    value: state.orderDate ? new Date(state.orderDate) : null,
                    change: (e) => {
                        state.orderDate = e.value;
                    }
                });
                orderDatePicker.obj.appendTo(orderDateRef.value);
            },
            refresh: () => {
                if (orderDatePicker.obj) {
                    orderDatePicker.obj.value = state.orderDate ? new Date(state.orderDate) : null;
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
            }
        };

        Vue.watch(
            () => state.orderDate,
            (newVal, oldVal) => {
                orderDatePicker.refresh();
                state.errors.orderDate = '';
            }
        );

        Vue.watch(
            () => state.customerId,
            (newVal, oldVal) => {
                customerListLookup.refresh();
                state.errors.customerId = '';
            }
        );

        //Vue.watch(
        //    () => state.orderStatus,
        //    (newVal, oldVal) => {
        //        salesOrderStatusListLookup.refresh();
        //        state.errors.orderStatus = '';
        //    }
        //);

        Vue.watch(
            () => state.taxId,
            (newVal, oldVal) => {
                taxListLookup.refresh();
            }
        );

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
                    groupSettings: { columns: ['customerName'] },
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
                        { field: 'orderDate', headerText: 'SO Date', width: 150, format: 'yyyy-MM-dd' },
                        { field: 'customerName', headerText: 'Customer', width: 200, minWidth: 200 },
                       // { field: 'orderStatusName', headerText: 'Status', width: 150, minWidth: 150 },
                        { field: 'taxName', headerText: 'Tax', width: 150, minWidth: 150 },
                        { field: 'afterTaxAmount', headerText: 'Total Amount', width: 150, minWidth: 150, format: 'N2' },
                        { field: 'createdAtUtc', headerText: 'Created At UTC', width: 150, format: 'yyyy-MM-dd HH:mm' },
                        { field: 'discount', headerText: 'Discount', width: 120, minWidth: 120, format: 'N2' },
                    ],
                    toolbar: [
                        'ExcelExport', 'Search',
                        { type: 'Separator' },
                        { text: 'Add', tooltipText: 'Add', prefixIcon: 'e-add', id: 'AddCustom' },
                        { text: 'Edit', tooltipText: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' },
                        { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' },
                        { type: 'Separator' },
                        { text: 'Print PDF', tooltipText: 'Print PDF', id: 'PrintPDFCustom' },
                    ],
                    beforeDataBound: () => { },
                    dataBound: function () {
                        mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'PrintPDFCustom'], false);
                        mainGrid.obj.autoFitColumns(['number', 'orderDate', 'customerName', 'taxName', 'discount', 'afterTaxAmount', 'createdAtUtc']);
                    },
                    excelExportComplete: () => { },
                    rowSelected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'PrintPDFCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'PrintPDFCustom'], false);
                        }
                    },
                    rowDeselected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'PrintPDFCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom', 'PrintPDFCustom'], false);
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
                            state.mainTitle = 'Add Sales Order';
                            resetFormState();
                            state.secondaryData = [];
                            secondaryGrid.refresh();
                            state.showComplexDiv = false;
                            mainModal.obj.show();
                        }

                        if (args.item.id === 'EditCustom') {
                            state.deleteMode = false;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Edit Sales Order';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                //state.orderDate = selectedRecord.orderDate ? new Date(selectedRecord.orderDate) : null;
                                state.description = selectedRecord.description ?? '';
                                state.customerId = selectedRecord.customerId ?? '';
                               // state.orderStatus = String(selectedRecord.orderStatus ?? '');
                                state.taxId = selectedRecord.taxId ?? null;
                                state.showComplexDiv = true;
                                taxListLookup.trackingChange = true;
                                state.discount = selectedRecord.discount ?? 0;
                                await methods.populateSecondaryData(selectedRecord.id);
                                secondaryGrid.refresh();

                                mainModal.obj.show();
                            }
                        }

                        if (args.item.id === 'DeleteCustom') {
                            state.deleteMode = true;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Delete Sales Order?';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                //state.orderDate = selectedRecord.orderDate ? new Date(selectedRecord.orderDate) : null;
                                state.description = selectedRecord.description ?? '';
                                state.customerId = selectedRecord.customerId ?? '';
                              //  state.orderStatus = String(selectedRecord.orderStatus ?? '');
                                state.taxId = selectedRecord.taxId ?? null;
                                state.showComplexDiv = false;
                                state.discount = selectedRecord.discount ?? 0;

                                await methods.populateSecondaryData(selectedRecord.id);
                                secondaryGrid.refresh();

                                mainModal.obj.show();
                            }
                        }

                        if (args.item.id === 'PrintPDFCustom') {
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                window.open('/SalesOrders/SalesOrderPdf?id=' + (selectedRecord.id ?? ''), '_blank');
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

        const secondaryGrid = {
            obj: null,
            create: async (dataSource) => {
                secondaryGrid.obj = new ej.grids.Grid({
                    height: 400,
                    dataSource: dataSource,
                    editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, showDeleteConfirmDialog: true, mode: 'Normal', allowEditOnDblClick: true },
                    allowFiltering: true,
                    allowSorting: true,
                    allowSelection: true,
                    allowGrouping: false,
                    allowTextWrap: true,
                    allowResizing: true,
                    allowPaging: false,
                    allowExcelExport: true,
                    filterSettings: { type: 'Excel' },
                    sortSettings: { columns: [{ field: 'productName', direction: 'Descending' }] },
                    pageSettings: { currentPage: 1, pageSize: 50, pageSizes: ["10", "20", "50", "100", "200", "All"] },
                    selectionSettings: { persistSelection: true, type: 'Single' },
                    autoFit: false,
                    showColumnMenu: true,
                    gridLines: 'Horizontal',
                    columns: [
                        { type: 'checkbox', width: 60 },
                        {
                            field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false
                        },
                        {
                            field: 'productId',
                            headerText: 'Product',
                            width: 250,
                            validationRules: { required: true },
                            disableHtmlEncode: false,
                            valueAccessor: (field, data, column) => {
                                const product = state.productListLookupData.find(item => item.id === data[field]);
                                return product ? `${product.name}` : '';
                            },
                            editType: 'dropdownedit',
                            edit: {
                                create: () => {
                                    let productElem = document.createElement('input');
                                    return productElem;
                                },
                                read: () => {
                                    return productObj.value;
                                },
                                destroy: () => {
                                    productObj.destroy();
                                },
                                write: async (args) => {
                                    productObj = new ej.dropdowns.DropDownList({
                                        dataSource: state.productListLookupData,
                                        fields: { value: 'id', text: 'name' },
                                        value: args.rowData.productId,
                                        placeholder: 'Select a Product',
                                        filterBarPlaceholder: 'Search by Name or Barcode',
                                        allowFiltering: true,
                                        filtering: async (e) => {
                                            e.preventDefaultAction = true;
                                            let query = new ej.data.Query();
                                            if (e.text && e.text.trim() !== '') {
                                                const searchText = e.text.toLowerCase();
                                                let namePredicate = new ej.data.Predicate('name', 'contains', searchText, true);
                                                let barcodePredicate = new ej.data.Predicate('barcode', 'contains', searchText, true);
                                                query = query.where(namePredicate.or(barcodePredicate));

                                                let filteredData = state.productListLookupData.filter(item =>
                                                    item.name.toLowerCase().includes(searchText) ||
                                                    (item.barcode && item.barcode.toLowerCase().includes(searchText))
                                                );

                                                if (filteredData.length === 0) {
                                                    try {
                                                        const response = await services.getProductByBarcode(searchText);
                                                        if (response.data.code === 200 && response.data.content.data) {
                                                            const product = response.data.content.data;
                                                            const existingProduct = state.productListLookupData.find(p => p.id === product.id);
                                                            if (!existingProduct) {
                                                                state.productListLookupData.push(product);
                                                                filteredData.push(product);
                                                            }
                                                        }
                                                    } catch (error) {
                                                        console.error('API search error:', error);
                                                    }
                                                }
                                                e.updateData(filteredData, query);
                                            } else {
                                                e.updateData(state.productListLookupData, query);
                                            }
                                        },
                                        change: async (e) => {
                                            const selectedProduct = state.productListLookupData.find(item => item.id === e.value);
                                            if (selectedProduct) {
                                                args.rowData.productId = selectedProduct.id;
                                                if (barcodeObj) {
                                                    barcodeObj.value = selectedProduct.barcode || '';
                                                }
                                                if (numberObj) {
                                                    numberObj.value = selectedProduct.number;
                                                }
                                                if (priceObj) {
                                                    priceObj.value = selectedProduct.unitPrice || 0;
                                                }
                                                if (summaryObj) {
                                                    summaryObj.value = selectedProduct.description || '';
                                                }
                                                if (quantityObj) {
                                                    quantityObj.value = 1;
                                                    const basePrice = selectedProduct.unitPrice || 0;
                                                    const vatPercentage = selectedProduct.vatPercentage || 0;
                                                    const vatAmount = basePrice * (vatPercentage / 100);
                                                    const total = (basePrice + vatAmount) * quantityObj.value;
                                                    if (totalPriceObj) {
                                                        totalPriceObj.value = total;
                                                    }
                                                }
                                                // Save the new item to the grid
                                                const salesOrderId = state.id;
                                                const userId = StorageManager.getUserId();
                                                await services.createSecondaryData(
                                                    selectedProduct.unitPrice || 0,
                                                    1,
                                                    selectedProduct.description || '',
                                                    selectedProduct.id,
                                                    salesOrderId,
                                                    userId
                                                );
                                                await methods.populateSecondaryData(salesOrderId);
                                                secondaryGrid.refresh();

                                                Swal.fire({
                                                    icon: 'success',
                                                    title: 'Product Added',
                                                    text: `Added ${selectedProduct.name} via selection`,
                                                    timer: 1000,
                                                    showConfirmButton: false
                                                });
                                            }
                                        },
                                        floatLabelType: 'Never'
                                    });
                                    productObj.appendTo(args.element);
                                }
                            }
                        },
                        {
                            field: 'barcode',
                            headerText: 'Barcode',
                            width: 150,
                            allowEditing: false,
                            valueAccessor: (field, data, column) => {
                                const product = state.productListLookupData.find(item => item.id === data['productId']);
                                return product ? (product.barcode || '') : '';
                            },
                            edit: {
                                create: () => {
                                    let barcodeElem = document.createElement('input');
                                    return barcodeElem;
                                },
                                read: () => {
                                    return barcodeObj.value;
                                },
                                destroy: () => {
                                    barcodeObj.destroy();
                                },
                                write: (args) => {
                                    barcodeObj = new ej.inputs.TextBox({
                                        value: args.rowData.barcode || '',
                                        readonly: true
                                    });
                                    barcodeObj.appendTo(args.element);
                                }
                            }
                        },
                        {
                            field: 'unitPrice',
                            headerText: 'Unit Price',
                            width: 200,
                            validationRules: { required: true, min: 0 },
                            type: 'number',
                            format: 'N2',
                            textAlign: 'Right',
                            edit: {
                                create: () => {
                                    let priceElem = document.createElement('input');
                                    return priceElem;
                                },
                                read: () => {
                                    return priceObj.value;
                                },
                                destroy: () => {
                                    priceObj.destroy();
                                },
                                write: (args) => {
                                    priceObj = new ej.inputs.NumericTextBox({
                                        value: args.rowData.unitPrice ?? 0,
                                        min: 0,
                                        change: (e) => {
                                            if (quantityObj && totalPriceObj) {
                                                const selectedProduct = state.productListLookupData.find(item => item.id === args.rowData.productId);
                                                const vatPercentage = selectedProduct?.vatPercentage || 0;
                                                const vatAmount = e.value * (vatPercentage / 100);
                                                const total = (e.value + vatAmount) * quantityObj.value;
                                                totalPriceObj.value = total;
                                            }
                                        }
                                    });
                                    priceObj.appendTo(args.element);
                                }
                            }
                        },
                        {
                            field: 'quantity',
                            headerText: 'Quantity',
                            width: 200,
                            validationRules: {
                                required: true,
                                custom: [(args) => {
                                    return args['value'] > 0;
                                }, 'Must be a positive number and not zero']
                            },
                            type: 'number',
                            format: 'N2',
                            textAlign: 'Right',
                            edit: {
                                create: () => {
                                    let quantityElem = document.createElement('input');
                                    return quantityElem;
                                },
                                read: () => {
                                    return quantityObj.value;
                                },
                                destroy: () => {
                                    quantityObj.destroy();
                                },
                                write: (args) => {
                                    quantityObj = new ej.inputs.NumericTextBox({
                                        value: args.rowData.quantity ?? 0,
                                        min: 1,
                                        change: (e) => {
                                            if (priceObj && totalPriceObj) {
                                                const selectedProduct = state.productListLookupData.find(item => item.id === args.rowData.productId);
                                                const vatPercentage = selectedProduct?.vatPercentage || 0;
                                                const vatAmount = priceObj.value * (vatPercentage / 100);
                                                const total = (priceObj.value + vatAmount) * e.value;
                                                totalPriceObj.value = total;
                                            }
                                        }
                                    });
                                    quantityObj.appendTo(args.element);
                                }
                            }
                        },
                        {
                            field: 'vatName',
                            headerText: 'VAT',
                            width: 150,
                            allowEditing: false,
                            valueAccessor: (field, data, column) => {
                                const product = state.productListLookupData.find(item => item.id === data['productId']);
                                return product ? product.vatName || 'Not Set' : 'Not Set';
                            }
                        },
                        {
                            field: 'totalPrice',
                            headerText: 'Total Price',
                            width: 200,
                            type: 'number',
                            format: 'N2',
                            textAlign: 'Right',
                            allowEditing: false,
                            valueAccessor: (field, data, column) => {
                                const product = state.productListLookupData.find(item => item.id === data['productId']);
                                const unitPrice = data['unitPrice'] || (product ? product.unitPrice : 0);
                                const quantity = data['quantity'] || 0;
                                const vatPercentage = product?.vatPercentage || 0;
                                const vatAmount = unitPrice * (vatPercentage / 100);
                                return (unitPrice + vatAmount) * quantity;
                            },
                            edit: {
                                create: () => {
                                    let totalPriceElem = document.createElement('input');
                                    return totalPriceElem;
                                },
                                read: () => {
                                    return totalPriceObj.value;
                                },
                                destroy: () => {
                                    totalPriceObj.destroy();
                                },
                                write: (args) => {
                                    totalPriceObj = new ej.inputs.NumericTextBox({
                                        value: args.rowData.totalPrice ?? 0,
                                        readonly: true
                                    });
                                    totalPriceObj.appendTo(args.element);
                                }
                            }
                        },
                        {
                            field: 'productNumber',
                            headerText: 'Product Number',
                            allowEditing: false,
                            width: 180,
                            edit: {
                                create: () => {
                                    let numberElem = document.createElement('input');
                                    return numberElem;
                                },
                                read: () => {
                                    return numberObj.value;
                                },
                                destroy: () => {
                                    numberObj.destroy();
                                },
                                write: (args) => {
                                    numberObj = new ej.inputs.TextBox();
                                    numberObj.value = args.rowData.productNumber;
                                    numberObj.readonly = true;
                                    numberObj.appendTo(args.element);
                                }
                            }
                        },
                        {
                            field: 'summary',
                            headerText: 'Summary',
                            width: 200,
                            edit: {
                                create: () => {
                                    let summaryElem = document.createElement('input');
                                    return summaryElem;
                                },
                                read: () => {
                                    return summaryObj.value;
                                },
                                destroy: () => {
                                    summaryObj.destroy();
                                },
                                write: (args) => {
                                    summaryObj = new ej.inputs.TextBox();
                                    summaryObj.value = args.rowData.summary;
                                    summaryObj.appendTo(args.element);
                                }
                            }
                        }
                    ],
                    toolbar: [
                        'ExcelExport',
                        { type: 'Separator' },
                        'Add', 'Edit', 'Delete', 'Update', 'Cancel',
                        'Search'
                    ],
                    beforeDataBound: () => { },
                    dataBound: function () { },
                    excelExportComplete: () => { },
                    rowSelected: () => {
                        if (secondaryGrid.obj.getSelectedRecords().length == 1) {
                            secondaryGrid.obj.toolbarModule.enableItems(['Edit'], true);
                        } else {
                            secondaryGrid.obj.toolbarModule.enableItems(['Edit'], false);
                        }
                    },
                    rowDeselected: () => {
                        if (secondaryGrid.obj.getSelectedRecords().length == 1) {
                            secondaryGrid.obj.toolbarModule.enableItems(['Edit'], true);
                        } else {
                            secondaryGrid.obj.toolbarModule.enableItems(['Edit'], false);
                        }
                    },
                    rowSelecting: () => {
                        if (secondaryGrid.obj.getSelectedRecords().length) {
                            secondaryGrid.obj.clearSelection();
                        }
                    },
                    toolbarClick: (args) => {
                        if (args.item.id === 'SecondaryGrid_excelexport') {
                            secondaryGrid.obj.excelExport();
                        }
                    },
                    actionComplete: async (args) => {
                        if (args.requestType === 'save' && args.action === 'add') {
                            const salesOrderId = state.id;
                            const userId = StorageManager.getUserId();
                            const data = args.data;

                            await services.createSecondaryData(data?.unitPrice, data?.quantity, data?.summary, data?.productId, salesOrderId, userId);
                            await methods.populateSecondaryData(salesOrderId);
                            secondaryGrid.refresh();

                            Swal.fire({
                                icon: 'success',
                                title: 'Save Successful',
                                timer: 2000,
                                showConfirmButton: false
                            });
                        }
                        if (args.requestType === 'save' && args.action === 'edit') {
                            const salesOrderId = state.id;
                            const userId = StorageManager.getUserId();
                            const data = args.data;

                            await services.updateSecondaryData(data?.id, data?.unitPrice, data?.quantity, data?.summary, data?.productId, salesOrderId, userId);
                            await methods.populateSecondaryData(salesOrderId);
                            secondaryGrid.refresh();

                            Swal.fire({
                                icon: 'success',
                                title: 'Save Successful',
                                timer: 2000,
                                showConfirmButton: false
                            });
                        }
                        if (args.requestType === 'delete') {
                            const salesOrderId = state.id;
                            const userId = StorageManager.getUserId();
                            const data = args.data[0];

                            await services.deleteSecondaryData(data?.id, userId);
                            await methods.populateSecondaryData(salesOrderId);
                            secondaryGrid.refresh();

                            Swal.fire({
                                icon: 'success',
                                title: 'Delete Successful',
                                timer: 2000,
                                showConfirmButton: false
                            });
                        }

                        await methods.populateMainData();
                        mainGrid.refresh();
                        await methods.refreshPaymentSummary(state.id);
                    }
                });
                secondaryGrid.obj.appendTo(secondaryGridRef.value);
            },
            refresh: () => {
                secondaryGrid.obj.setProperties({ dataSource: state.secondaryData });
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

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['SalesOrders']);
                await SecurityManager.validateToken();

                await methods.populateMainData();
                await mainGrid.create(state.mainData);

                mainModal.create();
                mainModalRef.value?.addEventListener('hidden.bs.modal', methods.onMainModalHidden);
                await methods.populateCustomerListLookupData();
                customerListLookup.create();
                //await methods.populateSalesOrderStatusListLookupData();
                //salesOrderStatusListLookup.create();
                await methods.populateTaxListLookupData();
                taxListLookup.create();
                orderDatePicker.create();
                numberText.create();
                await methods.populateProductListLookupData();
                await secondaryGrid.create(state.secondaryData);
            } catch (e) {
                console.error('page init error:', e);
            }
        });

        Vue.onUnmounted(() => {
            mainModalRef.value?.removeEventListener('hidden.bs.modal', methods.onMainModalHidden);
        });

        return {
            mainGridRef,
            mainModalRef,
            orderDateRef,
            numberRef,
            customerIdRef,
            //orderStatusRef,
            secondaryGridRef,
            barcodeRef,
            taxIdRef,
            state,
            methods,
            handler: {
                handleSubmit: methods.handleFormSubmit
            }
        };
    }
};

Vue.createApp(App).mount('#app');
