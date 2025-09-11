import React, { useState, useEffect } from 'react';
import {
  Input,
  Button,
  Field,
  Dropdown,
  Option,
  Checkbox,
  makeStyles,
  shorthands,
  Text
} from '@fluentui/react-components';
import { Search24Regular, Filter24Regular } from '@fluentui/react-icons';
import type { NotesPagedRequest, SortField } from '../types/Note';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    ...shorthands.gap('16px'),
    ...shorthands.padding('16px'),
    backgroundColor: '#f9f9f9',
    ...shorthands.borderRadius('8px'),
    marginBottom: '20px'
  },
  row: {
    display: 'flex',
    alignItems: 'end',
    ...shorthands.gap('12px'),
    flexWrap: 'wrap'
  },
  searchBox: {
    flexGrow: 1,
    minWidth: '200px'
  },
  sortSection: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('8px')
  }
});

interface SearchFiltersProps {
  filters: NotesPagedRequest;
  onFiltersChange: (filters: NotesPagedRequest) => void;
  onSearch: () => void;
  loading?: boolean;
}

const SORT_OPTIONS: { value: SortField; label: string }[] = [
  { value: 'updatedAtUtc', label: 'Last Updated' },
  { value: 'createdAtUtc', label: 'Created Date' },
  { value: 'title', label: 'Title' },
  { value: 'content', label: 'Content' }
];

const SearchFilters: React.FC<SearchFiltersProps> = React.memo(({
  filters,
  onFiltersChange,
  onSearch,
  loading = false
}) => {
  const styles = useStyles();
  const [localSearch, setLocalSearch] = useState(filters.search || '');
  const [localTitle, setLocalTitle] = useState(filters.title || '');
  const [localContent, setLocalContent] = useState(filters.content || '');
  const [searchError, setSearchError] = useState<string | null>(null);

  useEffect(() => {
    if (!filters.search && !filters.title && !filters.content) {
      setLocalSearch('');
      setLocalTitle('');
      setLocalContent('');
    }
  }, [filters.search, filters.title, filters.content]);

  const handleChange = (field: keyof NotesPagedRequest, value: any) => {
    onFiltersChange({ ...filters, [field]: value });
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearchClick();
    }
  };

  const handleSearchClick = () => {
    setSearchError(null);
    
    if (localSearch && localSearch.length < 3) {
      setSearchError('Search must be at least 3 characters');
      return;
    }
    
    onFiltersChange({ 
      ...filters, 
      search: localSearch || undefined,
      title: localTitle || undefined,
      content: localContent || undefined
    });
    onSearch();
  };

  const handleClearFilters = () => {
    setLocalSearch('');
    setLocalTitle('');
    setLocalContent('');
    setSearchError(null);
    onFiltersChange({
      page: 1,
      pageSize: filters.pageSize || 10,
      sortBy: 'updatedAtUtc',
      sortDescending: true
    });
  };

  return (
    <div className={styles.container}>
      {searchError && (
        <Text style={{ color: '#d13438', fontSize: '12px', marginBottom: '8px' }}>
          {searchError}
        </Text>
      )}
      <div className={styles.row}>
        <Field label="Search" className={styles.searchBox}>
          <Input
            placeholder="Search notes by title or content (min 3 chars)..."
            value={localSearch}
            onChange={(e) => setLocalSearch(e.target.value)}
            onKeyPress={handleKeyPress}
            contentBefore={<Search24Regular />}
          />
        </Field>
        
        <Field label="Sort by">
          <Dropdown
            value={SORT_OPTIONS.find(opt => opt.value === filters.sortBy)?.label || 'Last Updated'}
            onOptionSelect={(_, data) => {
              const option = SORT_OPTIONS.find(opt => opt.label === data.optionText);
              if (option) handleChange('sortBy', option.value);
            }}
          >
            {SORT_OPTIONS.map(option => (
              <Option key={option.value} value={option.label}>
                {option.label}
              </Option>
            ))}
          </Dropdown>
        </Field>

        <div className={styles.sortSection}>
          <Checkbox
            checked={filters.sortDescending !== false}
            onChange={(_, data) => handleChange('sortDescending', data.checked)}
            label="Descending"
          />
        </div>

        <Button
          appearance="primary"
          icon={<Search24Regular />}
          onClick={handleSearchClick}
          disabled={loading}
        >
          Search
        </Button>

        <Button
          appearance="secondary"
          icon={<Filter24Regular />}
          onClick={handleClearFilters}
          disabled={loading}
        >
          Clear
        </Button>
      </div>

      <div className={styles.row}>
        <Field label="Filter by Title">
          <Input
            placeholder="Title contains..."
            value={localTitle}
            onChange={(e) => setLocalTitle(e.target.value)}
            onKeyPress={handleKeyPress}
          />
        </Field>

        <Field label="Filter by Content">
          <Input
            placeholder="Content contains..."
            value={localContent}
            onChange={(e) => setLocalContent(e.target.value)}
            onKeyPress={handleKeyPress}
          />
        </Field>

        <Field label="Page Size">
          <Dropdown
            value={filters.pageSize?.toString() || '10'}
            onOptionSelect={(_, data) => {
              handleChange('pageSize', parseInt(data.optionText || '10', 10));
            }}
          >
            <Option value="5">5</Option>
            <Option value="10">10</Option>
            <Option value="20">20</Option>
            <Option value="50">50</Option>
          </Dropdown>
        </Field>
      </div>
    </div>
  );
});

export default SearchFilters;