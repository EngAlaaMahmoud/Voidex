const App = {
    setup() {
        const state = Vue.reactive({
            mainData: []
        });

        const mainGridRef = Vue.ref(null);

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/InventoryTransaction/GetInventoryTransactionReport', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
        };

        const methods = {
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = (response?.data?.content?.data || []).map(item => ({
                    ...item,
                    // normalize dates
                    transactionDate: item.transactionDate ? new Date(item.transactionDate) : null,
                    movementDate: item.movementDate ? new Date(item.movementDate) : null,
                    createdAtUtc: item.createdAtUtc ? new Date(item.createdAtUtc) : null
                }));
            }
        };

        Vue.onMounted(async () => {
            try {
                // use TransactionReports role/segment so the page won't redirect
                await SecurityManager.authorizePage(['TransactionReports']);
                await SecurityManager.validateToken();

                await methods.populateMainData();
                await mainGrid.create(state.mainData);

            } catch (e) {
                console.error('page init error:', e);
            }
        });

        const numberFormat = (v) => {
            if (v === null || v === undefined || v === '') return '';
            const n = Number(v);
            if (isNaN(n)) return '';
            return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        };

        const mainGrid = {
            obj: null,
            create: async (dataSource) => {
                mainGrid.obj = new ej.grids.Grid({
                    height: '520px',
                    dataSource: dataSource,
                    enableRtl: true, // right-to-left layout
                    allowFiltering: true,
                    allowSorting: true,
                    allowSelection: true,
                    allowGrouping: false,
                    allowTextWrap: true,
                    allowResizing: true,
                    allowPaging: true,
                    allowExcelExport: true,
                    filterSettings: { type: 'CheckBox' },
                    sortSettings: {
                        columns: [
                            { field: 'movementDate', direction: 'Ascending' }
                        ]
                    },
                    pageSettings: { currentPage: 1, pageSize: 200, pageSizes: ["50", "100", "200", "500", "All"] },
                    selectionSettings: { persistSelection: true, type: 'Single' },
                    autoFit: true,
                    showColumnMenu: true,
                    gridLines: 'Both',
                    // Columns arranged R->L visually because enableRtl is true
                    columns: [
                        { type: 'checkbox', width: 60 },
                        { field: 'transactionDate', headerText: 'التاريخ', width: 110, format: 'yyyy-MM-dd', textAlign: 'Center' },
                        { field: 'moduleName', headerText: 'البيان', width: 160, customAttributes: { class: 'module-cell' } },
                        // purchase price (display normal numeric)
                        { field: 'purchasePrice', headerText: 'سعر الشراء', width: 110, textAlign: 'Right', format: 'N2' },

                        // incoming: display with leading '+' when value exists
                        {
                            field: 'incoming',
                            headerText: 'وارد(+)',
                            width: 90,
                            textAlign: 'Right',
                            customAttributes: { class: 'incoming-cell' },
                            valueAccessor: function (field, data, column) {
                                const v = data.incoming;
                                if (v === null || v === undefined || v === '') return '';
                                const n = Number(v);
                                if (isNaN(n)) return '';
                                return `+${n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
                            }
                        },

                        // outgoing: display with leading '-' when value exists
                        {
                            field: 'outgoing',
                            headerText: 'منصرف(-)',
                            width: 90,
                            textAlign: 'Right',
                            customAttributes: { class: 'outgoing-cell' },
                            valueAccessor: function (field, data, column) {
                                const v = data.outgoing;
                                if (v === null || v === undefined || v === '') return '';
                                const n = Number(v);
                                if (isNaN(n)) return '';
                                return `-${n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
                            }
                        },

                        // purchaseValue: show trailing '-' if negative (valueAccessor only affects display)
                        {
                            field: 'purchaseValue',
                            headerText: 'التكلفة',
                            width: 110,
                            textAlign: 'Right',
                            valueAccessor: function (field, data, column) {
                                const v = data.purchaseValue;
                                if (v === null || v === undefined || v === '') return '';
                                const n = Number(v);
                                if (isNaN(n)) return '';
                                if (n < 0) return `${Math.abs(n).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}-`;
                                return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                            }
                        },

                        // stock: trailing minus for negative, formatted
                        {
                            field: 'stock',
                            headerText: 'الرصيد',
                            width: 110,
                            textAlign: 'Right',
                            customAttributes: { class: 'stock-cell' },
                            valueAccessor: function (field, data, column) {
                                const v = data.stock;
                                if (v === null || v === undefined || v === '') return '';
                                const n = Number(v);
                                if (isNaN(n)) return '';
                                if (n < 0) return `${Math.abs(n).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}-`;
                                return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                            }
                        },

                        // balanceValue: trailing minus if negative
                        {
                            field: 'balanceValue',
                            headerText: 'قيمة الرصيد',
                            width: 130,
                            textAlign: 'Right',
                            customAttributes: { class: 'balance-cell' },
                            valueAccessor: function (field, data, column) {
                                const v = data.balanceValue;
                                if (v === null || v === undefined || v === '') return '';
                                const n = Number(v);
                                if (isNaN(n)) return '';
                                if (n < 0) return `${Math.abs(n).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}-`;
                                return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                            }
                        },

                        { field: 'productNumber', headerText: 'رقم الصنف', width: 120 },
                        { field: 'productName', headerText: 'اسم الصنف', width: 180, visible: false },
                        { field: 'moduleNumber', headerText: 'رقم المستند', width: 140 },
                        { field: 'statusName', headerText: 'الحالة', width: 120, visible: false },
                        { field: 'movementDate', headerText: 'Movement Date', width: 150, format: 'yyyy-MM-dd', visible: false },
                        { field: 'createdAtUtc', headerText: 'Created At', width: 150, format: 'yyyy-MM-dd HH:mm', visible: false }
                    ],
                    aggregates: [
                        {
                            columns: [
                                // aggregates still use underlying numeric fields
                                { type: 'Sum', field: 'incoming', groupFooterTemplate: 'إجمالي الوارد: ${Sum}', groupCaptionTemplate: 'وارد: ${Sum}', format: 'N2' },
                                { type: 'Sum', field: 'outgoing', groupFooterTemplate: 'إجمالي المنصرف: ${Sum}', groupCaptionTemplate: 'منصرف: ${Sum}', format: 'N2' },
                                {
                                    type: 'Custom',
                                    field: 'stock',
                                    groupFooterTemplate: 'الرصيد: ${Custom}',
                                    customAggregate: function (data) {
                                        if (data && data.result && data.result.length > 0) {
                                            const last = data.result[data.result.length - 1];
                                            return last.stock ?? 0;
                                        }
                                        return 0;
                                    },
                                    format: 'N2'
                                },
                                { type: 'Sum', field: 'purchaseValue', groupFooterTemplate: 'إجمالي التكلفة: ${Sum}', format: 'N2' }
                            ]
                        }
                    ],
                    toolbar: ['ExcelExport', 'Search', { type: 'Separator' }],
                    dataBound: function () {
                        // apply row-level styling (cells already display prefixed/trailed strings via valueAccessor)
                        try {
                            mainGrid.obj.autoFitColumns();

                            const rows = mainGrid.obj.getRows();
                            rows.forEach(tr => {
                                const incomingCell = tr.querySelector('.incoming-cell');
                                if (incomingCell) {
                                    incomingCell.style.backgroundColor = incomingCell.textContent.trim() ? '#ccffcc' : '';
                                }

                                const outgoingCell = tr.querySelector('.outgoing-cell');
                                if (outgoingCell) {
                                    outgoingCell.style.backgroundColor = outgoingCell.textContent.trim() ? '#ffcccc' : '';
                                }

                                const stockCell = tr.querySelector('.stock-cell');
                                if (stockCell) {
                                    const raw = stockCell.textContent.replace(/,/g, '').replace('+', '').replace('-', '');
                                    const num = Number(raw);
                                    if (!isNaN(num)) {
                                        if (stockCell.textContent.includes('-')) {
                                            // negative (we display trailing '-') => mark red
                                            stockCell.style.backgroundColor = '#ffcccc';
                                            stockCell.style.fontWeight = 'bold';
                                        } else if (num === 0) {
                                            stockCell.style.backgroundColor = '#ffffcc';
                                        } else {
                                            stockCell.style.backgroundColor = '';
                                            stockCell.style.fontWeight = '';
                                        }
                                    }
                                }

                                const moduleCell = tr.querySelector('.module-cell');
                                if (moduleCell) {
                                    const txt = moduleCell.textContent.trim();
                                    if (txt.includes('بيع')) moduleCell.style.backgroundColor = '#ffff66';
                                    else if (txt.includes('شراء')) moduleCell.style.backgroundColor = '#cce5ff';
                                    else if (txt.includes('تحويل')) moduleCell.style.backgroundColor = '#e6f7e6';
                                    else moduleCell.style.backgroundColor = '';
                                }

                                const balanceCell = tr.querySelector('.balance-cell');
                                if (balanceCell) {
                                    // balance already formatted by valueAccessor
                                }
                            });
                        } catch (err) {
                            console.warn('dataBound styling error', err);
                        }
                    },
                    toolbarClick: (args) => {
                        if (args.item && args.item.id && args.item.id.toLowerCase().includes('excelexport')) {
                            mainGrid.obj.excelExport({
                                fileName: 'InventoryTransactionReport_' + new Date().toISOString().split('T')[0] + '.xlsx'
                            });
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

        return {
            mainGridRef,
            state,
        };
    }
};

Vue.createApp(App).mount('#app');