import { createSlice } from '@reduxjs/toolkit';


const commonSlice = createSlice({
    name: 'commonSlice',
    initialState: {
        expandedNavbar: true,
    },
    reducers: {
        setExpandedNavbar: state => {
            state.expandedNavbar = !state.expandedNavbar;
        }
    }
})

export const { setExpandedNavbar } = commonSlice.actions;
export const commonReducer = commonSlice.reducer;