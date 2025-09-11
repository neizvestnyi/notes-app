import React from 'react';
import {
  Button,
  Text,
  makeStyles,
  shorthands
} from '@fluentui/react-components';
import {
  ChevronLeft24Regular,
  ChevronRight24Regular,
  Previous24Regular,
  Next24Regular
} from '@fluentui/react-icons';
import type { PaginatedResponse } from '../types/Note';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    ...shorthands.padding('16px', '0'),
    flexWrap: 'wrap',
    ...shorthands.gap('8px')
  },
  info: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('16px'),
    flexWrap: 'wrap'
  },
  controls: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('8px')
  },
  pageNumbers: {
    display: 'flex',
    alignItems: 'center',
    ...shorthands.gap('4px')
  }
});

interface PaginationControlsProps {
  pagination: PaginatedResponse<any>;
  onPageChange: (page: number) => void;
  loading?: boolean;
}

const PaginationControls: React.FC<PaginationControlsProps> = ({
  pagination,
  onPageChange,
  loading = false
}) => {
  const styles = useStyles();

  if (pagination.totalCount === 0) {
    return null;
  }

  const generatePageNumbers = () => {
    const pages: (number | string)[] = [];
    const { page, totalPages } = pagination;
    
    if (totalPages > 0) {
      pages.push(1);
    }
    
    if (page > 4) {
      pages.push('...');
    }
    
    const start = Math.max(2, page - 2);
    const end = Math.min(totalPages - 1, page + 2);
    
    for (let i = start; i <= end; i++) {
      if (!pages.includes(i)) {
        pages.push(i);
      }
    }
    
    if (page < totalPages - 3) {
      if (!pages.includes('...')) {
        pages.push('...');
      }
    }
    
    if (totalPages > 1 && !pages.includes(totalPages)) {
      pages.push(totalPages);
    }
    
    return pages;
  };

  return (
    <div className={styles.container}>
      <div className={styles.info}>
        <Text>
          Showing {pagination.firstItemIndex}-{pagination.lastItemIndex} of {pagination.totalCount} notes
        </Text>
        {pagination.search && (
          <Text style={{ fontStyle: 'italic', color: '#666' }}>
            Filtered by: "{pagination.search}"
          </Text>
        )}
      </div>

      <div className={styles.controls}>
        <Button
          icon={<Previous24Regular />}
          appearance="subtle"
          disabled={loading || !pagination.hasPreviousPage}
          onClick={() => onPageChange(1)}
          title="First page"
        />
        
        <Button
          icon={<ChevronLeft24Regular />}
          appearance="subtle"
          disabled={loading || !pagination.hasPreviousPage}
          onClick={() => onPageChange(pagination.previousPage!)}
          title="Previous page"
        />

        <div className={styles.pageNumbers}>
          {generatePageNumbers().map((pageNum, index) => (
            typeof pageNum === 'number' ? (
              <Button
                key={pageNum}
                appearance={pageNum === pagination.page ? 'primary' : 'subtle'}
                disabled={loading}
                onClick={() => onPageChange(pageNum)}
                size="small"
              >
                {pageNum}
              </Button>
            ) : (
              <Text key={`ellipsis-${index}`} style={{ padding: '0 4px' }}>
                {pageNum}
              </Text>
            )
          ))}
        </div>

        <Button
          icon={<ChevronRight24Regular />}
          appearance="subtle"
          disabled={loading || !pagination.hasNextPage}
          onClick={() => onPageChange(pagination.nextPage!)}
          title="Next page"
        />
        
        <Button
          icon={<Next24Regular />}
          appearance="subtle"
          disabled={loading || !pagination.hasNextPage}
          onClick={() => onPageChange(pagination.totalPages)}
          title="Last page"
        />
      </div>
    </div>
  );
};

export default PaginationControls;