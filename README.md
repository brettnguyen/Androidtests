import openpyxl
import pandas as pd
import tkinter as tk
from tkinter import filedialog
import os
from openpyxl import load_workbook
from openpyxl.utils.dataframe import dataframe_to_rows
from openpyxl.utils import get_column_letter
    
def process_file(file_paths):

    all_data = []
    all_original_data = []

     # Increase the possible names a column may be
    column_mappings = {
        'quantity': ['Quantity (Abs)', 'Quantity', 'Amount'],
        'ticker' : ['Ticker', 'Bloomberg Symbol', 'Symbol'],
        'side' : ['Side','Buy/Sell']
    }
    
    for file_path in file_paths:
        try:           

            # Extract the first string of file name
            filename = os.path.basename(file_path)
            first_string = filename.split(' ')[0] #Adjust split based on file name

            # load files 
            original_df = pd.read_excel(file_path)
            all_original_data.append(original_df)      

            #Columns Based on mappings
            for standard_name, possible_names in column_mappings.items():
                for possible_name in possible_names:
                    if possible_name in original_df.columns:
                        original_df.rename(columns={possible_name: standard_name}, inplace=True)

            
        
            if 'ticker' not in original_df.columns or 'side' not in original_df.columns:
                print(f"Missing required columns in file: {filename}")
                continue

            # Create the new dataframe structure 
            data = {
                'Key': [],
                'Ticker': [],
                'Side Flag': [],
                'Sum of QTY': [],
                'Order QTY': [],
                'Diff': []           
               
            }

            # Populate the new datafrane with the required structure
            for index, row in original_df.iterrows():
                side_value = row['side']
                if 'SHORT' in side_value.upper():
                    side_flag = 'S'
                else:
                    side_flag = 'B' if side_value.lower() == 'buy' else 'S'
                key = f"{first_string}{row['ticker']}{side_flag}"
                data['Key'].append(key)
                data['Ticker'].append(row['ticker'])
                data['Side Flag'].append(side_flag)
                data['Sum of QTY'].append(row['quantity'])
                data['Order QTY'].append(row['quantity'])
                data['Diff'].append(0)
          

            # List all data
            all_data.append(pd.DataFrame(data))

        except Exception as e:
            print(f"Error processing file {filename}:{e}")

    if not all_data:
        print("No valid data to concatenate. Exiting.")
        return

    #Create new dateframe
    combined_orders_df = pd.concat(all_data, ignore_index=True)

    all_original_df = pd.concat(all_original_data, ignore_index=True)

    # Save dateframe to Excel file
    desktop_path = os.path.join(os.path.expanduser("~"), "Desktop")
    save_path = os.path.join(desktop_path, "Orders_Simplified_file.xlsx")

    # Ensure directory exists
    if not os.path.exists(desktop_path):
        os.makedirs(desktop_path)
    
    with pd.ExcelWriter(save_path, engine='openpyxl') as writer:
        all_original_df.to_excel(writer, sheet_name='Orginal Data', index=False)
        combined_orders_df.to_excel(writer, sheet_name='Combined Data', index=False)

   ## combined_orders_df.to_excel(save_path, index=False, engine ='openpyxl')

    # Hide Unsused rows and columns in Excel?
    Wb = load_workbook(save_path)
    ws_combined = Wb['Combined Data']

    # Hide all columns after the last used column
    for col in range(combined_orders_df.shape[1] + 1, ws_combined.max_column + 1):
        ws_combined.column_dimensions[openpyxl.utils.get_column_letter(col)].hidden = True
    
    # Hide all rows after last used row
    for row in range(combined_orders_df.shape[0] + 2, ws_combined.max_row + 1):
        ws_combined.row_dimensions[row].hidden = True
    
    Wb.save(save_path)
    print(f"File saved as {save_path}")
    return combined_orders_df

def process_additional_file(combined_df, additional_bloom_df):
    additional_bloom_df = pd.read_excel(additional_bloom_df)

   
    # Update 'Sum of QTY' in the combined DataFrame
    for index, row in combined_df.iterrows():
        matching_row = additional_bloom_df[additional_bloom_df['Account Code'] == row['Key'][:len(row['Key']) - 2]]
        if not matching_row.empty:
            combined_df.at[index, 'Sum of QTY'] = matching_row['Raw Amount'].values[0]
    
    return combined_df
# Create tkinter to grab/choose files
root = tk.Tk()
root.withdraw()

# Opens file
file_paths = filedialog.askopenfilenames(filetypes=[("Excel files", "*.xlsx")])

# Process file
while True:
    if file_paths:
        combined_df = process_file(file_paths)
        
        # Open Additional file
        additional_file_path = filedialog.askopenfilename(filetypes=[("Excel files", "*.xlsx")])

        if additional_file_path:
            combined_df = process_additional_file(combined_df, additional_file_path)

            desktop_path = os.path.join(os.path.expanduser("~"), "Desktop")
            save_path = os.path.join(desktop_path, "Reconciliation_file.xlsx")
            combined_df.to_excel(save_path, index=False, engine='openpyxl')
        else:
            print("No additional files selected")
    else:
        print("No files seleceted")
