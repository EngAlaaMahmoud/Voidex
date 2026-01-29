const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            deleteMode: false,
            productGroupListLookupData: [],
            unitMeasureListLookupData: [],
            vatListLookupData: [],
            //taxListLookupData: [],
            taxListData: [], // ← Add tax list data
            mainTitle: null,
            id: '',
            name: '',
            number: '',
            barcode: '',
            internalCode: '',
            gisEgsCode: '',
            model: '',
            unitPrice: 0,
            discount: 0,
            priceAfterDiscount: 0,
            description: '',
            productGroupId: null,
            unitMeasureId: null,
            vatId: null,
            productCompanyId: null,
            productCompanyMap: {},
            //taxId: null,
            physical: false,
            // New: Product taxes array
            productTaxes: [],
            errors: {
                name: '',
                unitPrice: '',
                productGroupId: '',
                unitMeasureId: '',
                vatId: '',
                //taxId: ''
            },
            isSubmitting: false
        });

        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const productGroupIdRef = Vue.ref(null);
        const unitMeasureIdRef = Vue.ref(null);
        const vatIdRef = Vue.ref(null);
        //const taxIdRef = Vue.ref(null);
        const nameRef = Vue.ref(null);
        const numberRef = Vue.ref(null);
        const barcodeRef = Vue.ref(null);
        const unitPriceRef = Vue.ref(null);


        // Add this new function to format currency
        const formatCurrency = (value) => {
            if (!value) return '0.00';
            return parseFloat(value).toLocaleString('en-US', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        };

        // Add this new function to get unique main codes from tax list
        const getUniqueMainCodes = () => {
            if (!state.taxListData || !state.taxListData.length) return [];
            const mainCodes = [...new Set(state.taxListData.map(tax => tax.mainCode))];
            return mainCodes.filter(code => code); // Remove empty/null
        };

        // Add this new function to get sub codes by main code
        const getSubCodesByMainCode = (mainCode) => {
            if (!mainCode || !state.taxListData || !state.taxListData.length) return [];
            return state.taxListData
                .filter(tax => tax.mainCode === mainCode)
                .map(tax => ({
                    code: tax.subCode,
                    description: tax.description || tax.taxCategoryName,
                    percentage: tax.percentage,
                    taxCategoryName: tax.taxCategoryName
                }));
        };

        // Add this new function to add a tax row
        const addTaxRow = () => {
            state.productTaxes.push({
                id: null,
                taxMainCode: '',
                taxSubCode: '',
                taxCategoryName: '',
                percentage: 0,
                taxValue: 0,
                taxId: null // Store the tax ID for backend
            });
        };

        // Add this new function to remove a tax row
        const removeTaxRow = (index) => {
            state.productTaxes.splice(index, 1);
            calculateTaxValues();
        };

        // Add this new function to handle main code change
        const onTaxMainCodeChange = (index) => {
            const taxRow = state.productTaxes[index];
            taxRow.taxSubCode = '';
            taxRow.taxCategoryName = '';
            taxRow.percentage = 0;
            taxRow.taxValue = 0;
            taxRow.taxId = null;
        };

        // Add this new function to handle sub code change
        const onTaxSubCodeChange = (index) => {
            const taxRow = state.productTaxes[index];
            const mainCode = taxRow.taxMainCode;
            const subCode = taxRow.taxSubCode;

            if (mainCode && subCode) {
                const tax = state.taxListData.find(t =>
                    t.mainCode === mainCode && t.subCode === subCode
                );

                if (tax) {
                    taxRow.taxCategoryName = tax.taxCategoryName || tax.taxType || '';
                    taxRow.percentage = tax.percentage || 0;
                    taxRow.taxId = tax.id; // Store the tax ID
                    calculateTaxValue(index);
                }
            }
        };

        // Add this new function to calculate tax value for a row
        const calculateTaxValue = (index) => {
            const taxRow = state.productTaxes[index];
            const basePrice = state.priceAfterDiscount > 0 ? state.priceAfterDiscount : state.unitPrice;
            const percentage = parseFloat(taxRow.percentage) || 0;

            if (basePrice > 0 && percentage > 0) {
                taxRow.taxValue = (basePrice * percentage) / 100;
            } else {
                taxRow.taxValue = 0;
            }
        };

        // Add this new function to calculate all tax values
        const calculateTaxValues = () => {
            state.productTaxes.forEach((taxRow, index) => {
                calculateTaxValue(index);
            });
        };

        // Add this new function to calculate total tax percentage
        const calculateTotalTaxPercentage = () => {
            return state.productTaxes.reduce((total, tax) => {
                return total + (parseFloat(tax.percentage) || 0);
            }, 0);
        };

        // Add this new function to calculate total tax value
        const calculateTotalTaxValue = () => {
            return state.productTaxes.reduce((total, tax) => {
                return total + (parseFloat(tax.taxValue) || 0);
            }, 0);
        };

        // Add this new function to calculate price after taxes
        const calculatePriceAfterTaxes = () => {
            const basePrice = state.priceAfterDiscount > 0 ? state.priceAfterDiscount : state.unitPrice;
            return basePrice + calculateTotalTaxValue();
        };


        // Define mainModal FIRST to ensure it's available everywhere
        const mainModal = {
            obj: null,
            create: () => {
                if (mainModalRef.value) {
                    mainModal.obj = new bootstrap.Modal(mainModalRef.value, {
                        backdrop: 'static',
                        keyboard: false
                    });
                    console.log('Modal created successfully');
                } else {
                    console.error('Modal element not found');
                }
            },
            show: () => {
                if (mainModal.obj) {
                    mainModal.obj.show();
                } else {
                    console.error('Modal object is null');
                }
            },
            hide: () => {
                if (mainModal.obj) {
                    mainModal.obj.hide();
                }
            }
        };

        // ensure refs for discount and priceAfterDiscount targets
        // (these are created in DOM with ref attributes inside template)

        // Define lookup objects
        const productGroupListLookup = {
            obj: null,
            create: () => {
                if (state.productGroupListLookupData && Array.isArray(state.productGroupListLookupData) && state.productGroupListLookupData.length > 0) {
                    productGroupListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.productGroupListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a Product Group',
                        popupHeight: '200px',
                        change: (e) => {
                            state.productGroupId = e.value;
                            console.log('productGroup changed to', e.value);
                            // populate product companies from local map (no network)
                            methods.populateProductCompaniesFromMap(e.value);
                        }
                    });
                    productGroupListLookup.obj.appendTo(productGroupIdRef.value);
                } else {
                    console.warn('ProductGroup list lookup data is not available or invalid.');
                }
            },
            refresh: () => {
                if (productGroupListLookup.obj) {
                    productGroupListLookup.obj.value = state.productGroupId;
                }
            },
        };

        // product company lookup for selected group
        const productCompanyLookup = {
            obj: null,
            create: (dataSource) => {
                // destroy existing first
                productCompanyLookup.destroy();
                const elem = document.getElementById('productCompanyPlaceholder');
                if (!elem) return;
                // create a fresh input element inside placeholder
                elem.innerHTML = '<input id="productCompanyElem" />';
                const list = Array.isArray(dataSource) ? dataSource : [];
                productCompanyLookup.obj = new ej.dropdowns.DropDownList({
                    dataSource: list,
                    fields: { value: 'id', text: 'name' },
                    placeholder: 'Select Product Company',
                    popupHeight: '200px',
                    enabled: list.length > 0,
                    change: (e) => {
                        state.productCompanyId = e.value;
                    }
                });
                productCompanyLookup.obj.appendTo(document.getElementById('productCompanyElem'));
                // set initial value from state
                try { productCompanyLookup.obj.value = state.productCompanyId; } catch (err) { }
                // attach clear button
                const clearBtn = document.getElementById('productCompanyClearBtn');
                if (clearBtn) {
                    clearBtn.onclick = () => {
                        try { productCompanyLookup.obj.value = null; } catch {}
                        state.productCompanyId = null;
                    };
                }
            },
            refresh: () => {
                if (productCompanyLookup.obj) {
                    try { productCompanyLookup.obj.value = state.productCompanyId; } catch (err) { }
                }
            }
            ,
            destroy: () => {
                if (productCompanyLookup.obj) {
                    try { productCompanyLookup.obj.destroy(); } catch (err) { }
                    productCompanyLookup.obj = null;
                }
                const elem = document.getElementById('productCompanyPlaceholder');
                if (elem) elem.innerHTML = '';
            }
        };

        const unitMeasureListLookup = {
            obj: null,
            create: () => {
                if (state.unitMeasureListLookupData && Array.isArray(state.unitMeasureListLookupData) && state.unitMeasureListLookupData.length > 0) {
                    unitMeasureListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.unitMeasureListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a Unit Measure',
                        popupHeight: '200px',
                        change: (e) => {
                            state.unitMeasureId = e.value;
                        }
                    });
                    unitMeasureListLookup.obj.appendTo(unitMeasureIdRef.value);
                } else {
                    console.warn('UnitMeasure list lookup data is not available or invalid.');
                }
            },
            refresh: () => {
                if (unitMeasureListLookup.obj) {
                    unitMeasureListLookup.obj.value = state.unitMeasureId;
                }
            },
        };

        const vatListLookup = {
            obj: null,
            create: () => {
                if (state.vatListLookupData && Array.isArray(state.vatListLookupData) && state.vatListLookupData.length > 0) {
                    vatListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.vatListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a VAT',
                        popupHeight: '200px',
                        change: (e) => {
                            state.vatId = e.value;
                        }
                    });
                    vatListLookup.obj.appendTo(vatIdRef.value);
                } else {
                    console.warn('VAT list lookup data is not available or invalid.');
                    // Create empty dropdown as fallback
                    vatListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: [],
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'No VAT data available',
                        popupHeight: '200px',
                        enabled: false,
                        change: (e) => {
                            state.vatId = e.value;
                        }
                    });
                    vatListLookup.obj.appendTo(vatIdRef.value);
                }
            },
            refresh: () => {
                if (vatListLookup.obj) {
                    vatListLookup.obj.value = state.vatId;
                }
            },
        };

        //const taxListLookup = {
        //    obj: null,
        //    create: () => {
        //        if (state.taxListLookupData && Array.isArray(state.taxListLookupData) && state.taxListLookupData.length > 0) {
        //            taxListLookup.obj = new ej.dropdowns.DropDownList({
        //                dataSource: state.taxListLookupData,
        //                fields: { value: 'id', text: 'name' },
        //                placeholder: 'Select a Tax',
        //                popupHeight: '200px',
        //                change: (e) => {
        //                    state.taxId = e.value;
        //                }
        //            });
        //            taxListLookup.obj.appendTo(taxIdRef.value);
        //        } else {
        //            console.warn('Tax list lookup data is not available or invalid.');
        //            // Create empty dropdown as fallback
        //            taxListLookup.obj = new ej.dropdowns.DropDownList({
        //                dataSource: [],
        //                fields: { value: 'id', text: 'name' },
        //                placeholder: 'No Tax data available',
        //                popupHeight: '200px',
        //                enabled: false,
        //                change: (e) => {
        //                    state.taxId = e.value;
        //                }
        //            });
        //            taxListLookup.obj.appendTo(taxIdRef.value);
        //        }
        //    },
        //    refresh: () => {
        //        if (taxListLookup.obj) {
        //            taxListLookup.obj.value = state.taxId;
        //        }
        //    },
        //};

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
            },
        };

        const barcodeText = {
            obj: null,
            create: () => {
                barcodeText.obj = new ej.inputs.TextBox({
                    placeholder: 'Enter or Scan Barcode',
                });
                barcodeText.obj.appendTo(barcodeRef.value);
            },
            refresh: () => {
                if (barcodeText.obj) {
                    barcodeText.obj.value = state.barcode;
                }
            }
        };


        const unitPriceNumber = {
            obj: null,
            create: () => {
                unitPriceNumber.obj = new ej.inputs.NumericTextBox({
                    placeholder: 'Enter Unit Price',
                    decimals: 2,
                    format: 'N2',
                    min: 0
                });
                // append to input element
                const el = unitPriceRef.value;
                if (el) unitPriceNumber.obj.appendTo(el);
            },
            refresh: () => {
                if (unitPriceNumber.obj) {
                    unitPriceNumber.obj.value = state.unitPrice;
                }
            }
        };

        // use simple inputs for discount and price after discount (no extra widgets)

        const calculatePriceAfterDiscount = () => {
            const p = Number(state.unitPrice) || 0;
            const d = Number(state.discount) || 0;
            return Math.max(0, p - d);
        };

        const validateForm = function () {
            state.errors.name = '';
            state.errors.unitPrice = '';
            state.errors.productGroupId = '';
            state.errors.unitMeasureId = '';
            state.errors.vatId = '';
            //state.errors.taxId = '';

            let isValid = true;

            if (!state.name) {
                state.errors.name = 'Name is required.';
                isValid = false;
            }
            if (!state.unitPrice) {
                state.errors.unitPrice = 'Unit price is required.';
                isValid = false;
            }
            if (!state.productGroupId) {
                state.errors.productGroupId = 'Product group is required.';
                isValid = false;
            }
            if (!state.unitMeasureId) {
                state.errors.unitMeasureId = 'Unit measure is required.';
                isValid = false;
            }

            return isValid;
        };

        // Update resetFormState to clear taxes
        const resetFormState = () => {
            state.id = '';
            state.name = '';
            state.number = '';
            state.barcode = '';
            state.unitPrice = 0;
            state.description = '';
            state.productGroupId = null;
            state.productCompanyId = null;
            state.unitMeasureId = null;
            state.vatId = null;
            state.physical = true;
            state.productTaxes = []; // Clear taxes
            state.errors = {
                name: '',
                unitPrice: '',
                productGroupId: '',
                unitMeasureId: '',
                vatId: '',
            };
        };

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/Product/GetProductList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            createMainData: async (name, barcode, unitPrice, description,
                productGroupId, unitMeasureId, vatId, physical,
                createdById, productTaxes) => {
                try {
                    const response = await AxiosManager.post('/Product/CreateProduct', {
                        name,
                        barcode,
                        unitPrice,
                        description,
                        productGroupId,
                        productCompanyId: state.productCompanyId,
                        unitMeasureId,
                        vatId,
                        physical,
                        createdById,
                        internalCode: state.internalCode,
                        gisEgsCode: state.gisEgsCode,
                        model: state.model,
                        discount: state.discount,
                        priceAfterDiscount: state.priceAfterDiscount,
                        productTaxes: productTaxes // Send tax array
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },

            updateMainData: async (id, name, barcode, unitPrice, description,
                productGroupId, unitMeasureId, vatId, physical,
                updatedById, productTaxes) => {
                try {
                    const response = await AxiosManager.post('/Product/UpdateProduct', {
                        id,
                        name,
                        barcode,
                        unitPrice,
                        description,
                        productGroupId,
                        productCompanyId: state.productCompanyId,
                        unitMeasureId,
                        vatId,
                        physical,
                        updatedById,
                        internalCode: state.internalCode,
                        gisEgsCode: state.gisEgsCode,
                        model: state.model,
                        discount: state.discount,
                        priceAfterDiscount: state.priceAfterDiscount,
                        productTaxes: productTaxes // Send tax array
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            deleteMainData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/Product/DeleteProduct', {
                        id, deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getProductGroupListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/ProductGroup/GetProductGroupList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getProductCompaniesByGroup: async (groupId) => {
                try {
                    const response = await AxiosManager.get('/ProductGroup/GetProductCompaniesByGroup?groupId=' + groupId, {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getUnitMeasureListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/UnitMeasure/GetUnitMeasureList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getVatListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/Vat/GetVatList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            //getTaxListLookupData: async () => {
            //    try {
            //        const response = await AxiosManager.get('/Tax/GetTaxList', {});
            //        return response;
            //    } catch (error) {
            //        throw error;
            //    }
            //}

            getTaxListData: async () => {
                try {
                    const response = await AxiosManager.get('/Tax/GetTaxList', {});
                    return response;
                } catch (error) {
                    console.error('Failed to load tax list:', error);
                    throw error;
                }
            },
        };

        const methods = {
            populateProductGroupListLookupData: async () => {
                const response = await services.getProductGroupListLookupData();
                state.productGroupListLookupData = response?.data?.content?.data;
                // also clear product company selection
                state.productCompanyId = null;
            },
            populateUnitMeasureListLookupData: async () => {
                const response = await services.getUnitMeasureListLookupData();
                state.unitMeasureListLookupData = response?.data?.content?.data;
            },
            populateVatListLookupData: async () => {
                const response = await services.getVatListLookupData();
                state.vatListLookupData = response?.data?.content?.data;
            },

            populateProductCompaniesByGroup: async (groupId) => {
                if (!groupId) return;
                try {
                    const resp = await services.getProductCompaniesByGroup(groupId);
                    console.log('GetProductCompaniesByGroup response', resp);
                    const items = resp?.data?.content?.data ?? [];
                    if (!items || !items.length) {
                        console.warn('No product companies returned for group', groupId, items);
                    }
                    // render dropdown placeholder
                    const placeholder = document.getElementById('productCompanyPlaceholder');
                    if (placeholder) placeholder.innerHTML = '';
                    // fallback: if API returned empty, try to build items from loaded productGroupListLookupData (companyIds + companyNames)
                    let finalItems = items;
                    if ((!finalItems || !finalItems.length) && Array.isArray(state.productGroupListLookupData)) {
                        const grp = state.productGroupListLookupData.find(g => g.id === groupId);
                        if (grp && Array.isArray(grp.companyIds) && grp.companyIds.length) {
                            const names = (grp.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                            finalItems = grp.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' }));
                        }
                    }

                    productCompanyLookup.create(finalItems || []);
                } catch (err) {
                    console.error('failed loading product companies for group', err);
                }
            },
            populateTaxListData: async () => {
                try {
                    const response = await services.getTaxListData();
                    state.taxListData = response?.data?.content?.data || [];
                    console.log('Loaded tax list:', state.taxListData.length, 'items');
                } catch (error) {
                    console.error('Failed to load tax list:', error);
                    state.taxListData = [];
                }
            },


            populateProductCompaniesFromMap: async (groupId) => {
                if (!groupId) return;
                const mapping = state.productCompanyMap || {};
                const items = mapping[groupId] || [];
                if (!items || !items.length) {
                    // try to build from productGroupListLookupData
                    const grp = state.productGroupListLookupData.find(g => g.id === groupId);
                    if (grp && Array.isArray(grp.companyIds) && grp.companyIds.length) {
                        const names = (grp.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                        productCompanyLookup.create(grp.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' })));
                        return;
                    }
                }
                productCompanyLookup.create(items);
            },
            //populateTaxListLookupData: async () => {
            //    const response = await services.getTaxListLookupData();
            //    state.taxListLookupData = response?.data?.content?.data;
            //},
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = response?.data?.content?.data.map(item => ({
                    ...item,
                    internalCode: item.internalCode ?? item.InternalCode ?? '',
                    gisEgsCode: item.gisEgsCode ?? item.GisEgsCode ?? '',
                    companyName: item.companyName ?? item.CompanyName ?? '',
                    model: item.model ?? item.Model ?? '',
                    discount: item.discount ?? item.Discount ?? null,
                    priceAfterDiscount: item.priceAfterDiscount ?? item.PriceAfterDiscount ?? null,
                    serviceFee: item.serviceFee ?? item.ServiceFee ?? null,
                    additionalTax: item.additionalTax ?? item.AdditionalTax ?? null,
                    additionalFee: item.additionalFee ?? item.AdditionalFee ?? null,
                    createdAtUtc: new Date(item.createdAtUtc)
                }));
            },
            handleFormSubmit: async () => {
                state.isSubmitting = true;
                await new Promise(resolve => setTimeout(resolve, 200));

                if (!validateForm()) {
                    state.isSubmitting = false;
                    return;
                }

                try {
                    // Prepare product taxes for submission
                    const productTaxes = state.productTaxes
                        .filter(tax => tax.taxId) // Only include taxes with valid ID
                        .map(tax => ({
                            taxId: tax.taxId,
                            taxValue: tax.taxValue,
                            mainCode: tax.taxMainCode,
                            subCode: tax.taxSubCode,
                            percentage: tax.percentage
                        }));

                    const response = state.id === ''
                        ? await services.createMainData(
                            state.name,
                            state.barcode,
                            state.unitPrice,
                            state.description,
                            state.productGroupId,
                            state.unitMeasureId,
                            state.vatId,
                            state.physical,
                            StorageManager.getUserId(),
                            productTaxes
                        )
                        : state.deleteMode
                            ? await services.deleteMainData(state.id, StorageManager.getUserId())
                            : await services.updateMainData(
                                state.id,
                                state.name,
                                state.barcode,
                                state.unitPrice,
                                state.description,
                                state.productGroupId,
                                state.unitMeasureId,
                                state.vatId,
                                state.physical,
                                StorageManager.getUserId(),
                                productTaxes
                            );

                    // ... rest of existing handleFormSubmit logic ...
                } catch (error) {
                    // ... error handling ...
                } finally {
                    state.isSubmitting = false;
                }
            }
        };

        
        const handler = {
            handleSubmit: methods.handleFormSubmit
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
                    groupSettings: { columns: ['productGroupName'] },
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
                        { field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false },
                        { field: 'name', headerText: 'Name', width: 200, minWidth: 200 },
                        { field: 'number', headerText: 'Number', width: 150, minWidth: 150 },
                        { field: 'barcode', headerText: 'Barcode', width: 120, minWidth: 120 },
                        { field: 'internalCode', headerText: 'Internal Code', width: 120 },
                        { field: 'gisEgsCode', headerText: 'GIS / EGS Code', width: 120 },
                        { field: 'companyName', headerText: 'Company', width: 150 },
                        { field: 'productCompanyName', headerText: 'Company Name', width: 150 },
                        { field: 'model', headerText: 'Model', width: 150 },
                        { field: 'unitPrice', headerText: 'Unit Price', width: 100, format: 'N2' },
                        { field: 'discount', headerText: 'Discount', width: 100, format: 'N2' },
                        { field: 'priceAfterDiscount', headerText: 'Price After Disc.', width: 120, format: 'N2' },

                        // Tax columns based on categories
                        { field: 'serviceFee', headerText: 'Service Fee', width: 100, format: 'N2' },
                        { field: 'additionalTax', headerText: 'Additional Tax', width: 100, format: 'N2' },
                        { field: 'additionalFee', headerText: 'Additional Fee', width: 100, format: 'N2' },

                        { field: 'totalTaxes', headerText: 'Total Taxes', width: 100, format: 'N2' },
                        { field: 'finalPrice', headerText: 'Final Price', width: 120, format: 'N2' },

                        { field: 'productGroupName', headerText: 'Product Group', width: 150 },
                        { field: 'unitMeasureName', headerText: 'Unit Measure', width: 150 },
                        { field: 'vatName', headerText: 'VAT', width: 150 },
                        { field: 'physical', headerText: 'Physical', width: 150, displayAsCheckBox: true, type: 'boolean' },
                        { field: 'createdAtUtc', headerText: 'Created At UTC', width: 150, format: 'yyyy-MM-dd HH:mm' }
                    ],
                    toolbar: [
                        'ExcelExport', 'Search',
                        { type: 'Separator' },
                        { text: 'Add', tooltipText: 'Add', prefixIcon: 'e-add', id: 'AddCustom' },
                        { text: 'Edit', tooltipText: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' },
                        { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' },
                    ],
                    beforeDataBound: () => { },
                    dataBound: function () {
                        mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                        mainGrid.obj.autoFitColumns(['name', 'number', 'barcode', 'unitPrice', 'productGroupName', 'unitMeasureName', 'vatName', 'physical', 'createdAtUtc']);
                    },
                    excelExportComplete: () => { },
                    rowSelected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                        }
                    },
                    rowDeselected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
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
                            state.mainTitle = 'Add Product';
                            resetFormState();
                            // ensure any previous product company dropdown is cleared
                            productCompanyLookup.destroy();
                            mainModal.show();
                        }

                        if (args.item.id === 'EditCustom') {
                            state.deleteMode = false;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Edit Product';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.barcode = selectedRecord.barcode ?? '';
                                state.unitPrice = selectedRecord.unitPrice ?? '';
                                state.description = selectedRecord.description ?? '';
                            state.productGroupId = selectedRecord.productGroupId ?? '';
                            state.productCompanyId = selectedRecord.productCompanyId ?? null;
                                state.unitMeasureId = selectedRecord.unitMeasureId ?? '';
                                state.vatId = selectedRecord.vatId ?? '';
                                //state.taxId = selectedRecord.taxId ?? '';
                                state.physical = selectedRecord.physical ?? false;
                                // populate product companies for this group then show modal so dropdown appears with selected value
                                try {
                                    await methods.populateProductCompaniesByGroup(state.productGroupId);
                                } catch (e) {
                                    console.error('failed to populate product companies on edit', e);
                                }
                                mainModal.show();
                            }
                        }

                        if (args.item.id === 'DeleteCustom') {
                            state.deleteMode = true;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Delete Product?';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.barcode = selectedRecord.barcode ?? '';
                                state.unitPrice = selectedRecord.unitPrice ?? '';
                                state.description = selectedRecord.description ?? '';
                                state.productGroupId = selectedRecord.productGroupId ?? '';
                                state.unitMeasureId = selectedRecord.unitMeasureId ?? '';
                                state.vatId = selectedRecord.vatId ?? '';
                                //state.taxId = selectedRecord.taxId ?? '';
                                state.physical = selectedRecord.physical ?? false;
                                mainModal.show();
                            }
                        }
                    }
                });

                mainGrid.obj.appendTo(mainGridRef.value);
            },
            refresh: () => {
                if (mainGrid.obj) {
                    mainGrid.obj.setProperties({ dataSource: state.mainData });
                }
            }
        };

        Vue.watch(
            () => state.name,
            (newVal, oldVal) => {
                state.errors.name = '';
                if (nameText.obj) {
                    nameText.refresh();
                }
            }
        );

        Vue.watch(
            () => state.number,
            (newVal, oldVal) => {
                if (numberText.obj) {
                    numberText.refresh();
                }
            }
        );

        Vue.watch(
            () => state.barcode,
            (newVal, oldVal) => {
                if (barcodeText.obj) {
                    barcodeText.refresh();
                }
            }
        );

        Vue.watch(
            () => state.unitPrice,
            (newVal, oldVal) => {
                state.errors.unitPrice = '';
                if (unitPriceNumber.obj) {
                    unitPriceNumber.refresh();
                }
                // Recalculate price after discount and taxes
                state.priceAfterDiscount = calculatePriceAfterDiscount();
                calculateTaxValues();
            }
        );

        Vue.watch(
            () => state.discount,
            (newVal, oldVal) => {
                state.priceAfterDiscount = calculatePriceAfterDiscount();
                calculateTaxValues();
            }
        );

        Vue.watch(
            () => state.productGroupId,
            (newVal, oldVal) => {
                state.errors.productGroupId = '';
                if (productGroupListLookup.obj) {
                    productGroupListLookup.refresh();
                }
            }
        );

        Vue.watch(
            () => state.unitMeasureId,
            (newVal, oldVal) => {
                state.errors.unitMeasureId = '';
                if (unitMeasureListLookup.obj) {
                    unitMeasureListLookup.refresh();
                }
            }
        );

        Vue.watch(
            () => state.vatId,
            (newVal, oldVal) => {
                state.errors.vatId = '';
                if (vatListLookup.obj) {
                    vatListLookup.refresh();
                }
            }
        );

        //Vue.watch(
        //    () => state.taxId,
        //    (newVal, oldVal) => {
        //        state.errors.taxId = '';
        //        if (taxListLookup.obj) {
        //            taxListLookup.refresh();
        //        }
        //    }
        //);

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['Products']);
                await SecurityManager.validateToken();

                // Initialize modal FIRST before anything else
                mainModal.create();

                // Load main data and create grid
                await methods.populateMainData();
                await mainGrid.create(state.mainData);

                // Load lookup data
                await methods.populateProductGroupListLookupData();
                productGroupListLookup.create();

                // Build local productCompanyMap
                state.productCompanyMap = {};
                state.productGroupListLookupData.forEach(g => {
                    if (g && Array.isArray(g.companyIds) && g.companyIds.length) {
                        const names = (g.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                        state.productCompanyMap[g.id] = g.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' }));
                    }
                });

                await methods.populateUnitMeasureListLookupData();
                unitMeasureListLookup.create();

                await methods.populateVatListLookupData();
                vatListLookup.create();

                // Load tax list data
                await methods.populateTaxListData();

                // Create form controls
                nameText.create();
                numberText.create();
                barcodeText.create();
                unitPriceNumber.create();

                // Add modal event listener
                if (mainModalRef.value) {
                    mainModalRef.value.addEventListener('hidden.bs.modal', () => {
                        resetFormState();
                    });
                }

                console.log('Page initialization completed successfully');

            } catch (e) {
                console.error('Page init error:', e);
                Swal.fire({
                    icon: 'error',
                    title: 'Initialization Error',
                    text: 'Failed to initialize the page. Please refresh and try again.',
                    confirmButtonText: 'OK'
                });
            }
        });


        Vue.onUnmounted(() => {
            if (mainModalRef.value) {
                mainModalRef.value.removeEventListener('hidden.bs.modal', resetFormState);
            }
            // destroy numeric widgets
            try { discountNumber.destroy?.(); } catch {}
            try { priceAfterDiscountNumber.destroy?.(); } catch {}
        });

        return {
            mainGridRef,
            mainModalRef,
            productGroupIdRef,
            unitMeasureIdRef,
            vatIdRef,
            //taxIdRef,
            nameRef,
            numberRef,
            barcodeRef,
            unitPriceRef,
            state,
            handler,

            addTaxRow,
            removeTaxRow,
            onTaxMainCodeChange,
            onTaxSubCodeChange,
            getUniqueMainCodes,
            getSubCodesByMainCode,
            formatCurrency,
            calculateTotalTaxPercentage,
            calculateTotalTaxValue,
            calculatePriceAfterTaxes
        };
    }
};

Vue.createApp(App).mount('#app');