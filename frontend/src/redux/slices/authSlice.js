import { createSlice } from '@reduxjs/toolkit';


const loginSlice = createSlice({
    name: 'loginSlice',
    initialState: {
        email: "",
        password: "",
    },
    reducers: {
        setEmail: (state, action) => {
            state.expandedNavbar = action.payload;
        },
        
    }
})

export const { setExpandedNavbar } = commonSlice.actions;
export const commonReducer = commonSlice.reducer;